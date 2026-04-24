using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Reports.DTOs;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Services
{
    public class ExcelExportService : IExcelExportService
    {
        // ── Color palette ──────────────────────────────────────────────────────────
        private static readonly XLColor HeaderBg = XLColor.FromHtml("#1F3864");  // dark navy
        private static readonly XLColor HeaderFg = XLColor.FromHtml("#FFFFFF");
        private static readonly XLColor SubHeaderBg = XLColor.FromHtml("#2E75B6");  // mid blue
        private static readonly XLColor SubHeaderFg = XLColor.FromHtml("#FFFFFF");
        private static readonly XLColor TotalsBg = XLColor.FromHtml("#D6E4F0");
        private static readonly XLColor PositiveFg = XLColor.FromHtml("#1E6B1E");  // dark green
        private static readonly XLColor NegativeFg = XLColor.FromHtml("#C00000");  // dark red
        private static readonly XLColor AlternateRow = XLColor.FromHtml("#F2F7FB");
        private static readonly XLColor BorderColor = XLColor.FromHtml("#B4C7DC");

        // ── Revenue report ─────────────────────────────────────────────────────────

        public byte[] ExportRevenuByPeriod(
            RevenueByPeriodDto data,
            DateTime from,
            DateTime to,
            string grouping)
        {
            using var wb = new XLWorkbook();

            BuildRevenueSummarySheet(wb, data, from, to, grouping);
            BuildRevenueDetailSheet(wb, data);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private void BuildRevenueSummarySheet(
            XLWorkbook wb, RevenueByPeriodDto data,
            DateTime from, DateTime to, string grouping)
        {
            var ws = wb.Worksheets.Add("Revenue Summary");
            ws.ShowGridLines = false;

            // ── Title block ───────────────────────────────────────────────────────
            MergeAndStyle(ws, "A1:G1",
                $"Revenue Report — {from:MMM yyyy} to {to:MMM yyyy} ({grouping})",
                16, bold: true, bg: HeaderBg, fg: HeaderFg);

            MergeAndStyle(ws, "A2:G2",
                $"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                10, bold: false, bg: HeaderBg, fg: XLColor.FromHtml("#BDD7EE"));

            ws.Row(1).Height = 28;
            ws.Row(2).Height = 16;

            // ── KPI cards row ──────────────────────────────────────────────────────
            SetKpiCell(ws, "A4", "Total Collected (USD)", data.TotalRevenueUSD, PositiveFg);
            SetKpiCell(ws, "C4", "Total Pending (USD)", data.TotalPendingUSD, XLColor.FromHtml("#9C6500"));
            SetKpiCell(ws, "E4", "Total Overdue (USD)", data.TotalOverdueUSD, NegativeFg);
            SetKpiCell(ws, "G4", "Total Deals", data.TotalDeals, XLColor.FromHtml("#1F3864"), isCurrency: false);

            ws.Row(4).Height = 14;
            ws.Row(5).Height = 24;

            // ── Column headers ─────────────────────────────────────────────────────
            var headers = new[]
            {
            "Period", "Deals", "Payments",
            "Collected (USD)", "Pending (USD)", "Overdue (USD)", "Avg Deal (USD)"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(7, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Font.FontSize = 10;
                cell.Style.Fill.BackgroundColor = SubHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                cell.Style.Border.BottomBorderColor = HeaderBg;
            }

            ws.Row(7).Height = 20;

            // ── Data rows ──────────────────────────────────────────────────────────
            int row = 8;
            decimal totalCollected = 0, totalPending = 0, totalOverdue = 0;

            foreach (var r in data.Rows)
            {
                bool isAlt = (row % 2 == 0);
                var bg = isAlt ? AlternateRow : XLColor.White;

                SetDataCell(ws, row, 1, r.PeriodLabel, bg, isText: true);
                SetDataCell(ws, row, 2, r.DealCount, bg, isText: false);
                SetDataCell(ws, row, 3, r.PaymentCount, bg, isText: false);
                SetCurrencyCell(ws, row, 4, r.CollectedUSD, bg, PositiveFg);
                SetCurrencyCell(ws, row, 5, r.PendingUSD, bg, XLColor.FromHtml("#9C6500"));
                SetCurrencyCell(ws, row, 6, r.OverdueUSD, bg, r.OverdueUSD > 0 ? NegativeFg : XLColor.Black);
                SetCurrencyCell(ws, row, 7, r.AvgDealValueUSD, bg, XLColor.Black);

                totalCollected += r.CollectedUSD;
                totalPending += r.PendingUSD;
                totalOverdue += r.OverdueUSD;
                row++;
            }

            // ── Totals row ─────────────────────────────────────────────────────────
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontName = "Arial";
            ws.Cell(row, 2).FormulaA1 = $"SUM(B8:B{row - 1})";
            ws.Cell(row, 3).FormulaA1 = $"SUM(C8:C{row - 1})";
            ws.Cell(row, 4).FormulaA1 = $"SUM(D8:D{row - 1})";
            ws.Cell(row, 5).FormulaA1 = $"SUM(E8:E{row - 1})";
            ws.Cell(row, 6).FormulaA1 = $"SUM(F8:F{row - 1})";
            ws.Cell(row, 7).FormulaA1 = $"IFERROR(AVERAGE(G8:G{row - 1}),0)";

            for (int c = 1; c <= 7; c++)
            {
                var cell = ws.Cell(row, c);
                cell.Style.Fill.BackgroundColor = TotalsBg;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                cell.Style.Border.TopBorderColor = SubHeaderBg;
                if (c >= 4)
                    cell.Style.NumberFormat.Format = "$#,##0.00;($#,##0.00);\"-\"";
                else
                    cell.Style.NumberFormat.Format = "#,##0";
            }

            ws.Row(row).Height = 18;

            // ── Column widths ──────────────────────────────────────────────────────
            ws.Column(1).Width = 16;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 18;
            ws.Column(6).Width = 18;
            ws.Column(7).Width = 18;

            // Freeze header rows and first column
            ws.SheetView.FreezeRows(7);
            ws.SheetView.FreezeColumns(1);
        }

        private void BuildRevenueDetailSheet(XLWorkbook wb, RevenueByPeriodDto data)
        {
            var ws = wb.Worksheets.Add("Period Detail");
            ws.ShowGridLines = false;

            MergeAndStyle(ws, "A1:E1", "Period Detail Breakdown",
                14, bold: true, bg: SubHeaderBg, fg: XLColor.White);

            var headers = new[] { "Period", "Collected (USD)", "Pending (USD)", "Overdue (USD)", "Collection Rate" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = SubHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 4;
            foreach (var r in data.Rows)
            {
                bool isAlt = (row % 2 == 0);
                var bg = isAlt ? AlternateRow : XLColor.White;

                ws.Cell(row, 1).Value = r.PeriodLabel;
                ws.Cell(row, 2).Value = r.CollectedUSD;
                ws.Cell(row, 3).Value = r.PendingUSD;
                ws.Cell(row, 4).Value = r.OverdueUSD;

                // Collection rate formula — collected / (collected + pending + overdue)
                ws.Cell(row, 5).FormulaA1 =
                    $"IFERROR(B{row}/(B{row}+C{row}+D{row}),0)";

                for (int c = 1; c <= 5; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Fill.BackgroundColor = bg;
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Font.FontSize = 10;
                    if (c >= 2 && c <= 4)
                        cell.Style.NumberFormat.Format = "$#,##0.00;($#,##0.00);\"-\"";
                    if (c == 5)
                        cell.Style.NumberFormat.Format = "0.0%;-0.0%;\"-\"";
                }

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 16;
            ws.SheetView.FreezeRows(3);
        }

        // ── Employee performance report ────────────────────────────────────────────

        public byte[] ExportEmployeePerformance(EmployeePerformanceDto data)
        {
            using var wb = new XLWorkbook();

            BuildPerformanceSummarySheet(wb, data);
            BuildPipelineBreakdownSheet(wb, data);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private void BuildPerformanceSummarySheet(XLWorkbook wb, EmployeePerformanceDto data)
        {
            var ws = wb.Worksheets.Add("Employee Performance");
            ws.ShowGridLines = false;

            MergeAndStyle(ws, "A1:K1",
                $"Employee Performance Report — {data.PeriodStart:dd MMM yyyy} to {data.PeriodEnd:dd MMM yyyy}",
                15, bold: true, bg: HeaderBg, fg: HeaderFg);

            MergeAndStyle(ws, "A2:K2",
                $"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                10, bold: false, bg: HeaderBg, fg: XLColor.FromHtml("#BDD7EE"));

            ws.Row(1).Height = 26;
            ws.Row(2).Height = 14;

            var headers = new[]
            {
            "Employee", "Role", "Total Deals", "Closed", "Close Rate",
            "Deal Value (USD)", "Collected (USD)", "Avg Days to Close",
            "Active Clients", "Pipeline Stage", "Email"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = SubHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                cell.Style.Border.BottomBorderColor = HeaderBg;
            }

            ws.Row(4).Height = 30;

            int row = 5;
            foreach (var r in data.Rows)
            {
                bool isAlt = (row % 2 == 0);
                var bg = isAlt ? AlternateRow : XLColor.White;

                // Pipeline label — shows where most deals are concentrated
                var pipelineStage = GetDominantStage(r);

                ws.Cell(row, 1).Value = r.FullName;
                ws.Cell(row, 2).Value = r.Role;
                ws.Cell(row, 3).Value = r.TotalDeals;
                ws.Cell(row, 4).Value = r.ClosedCount;
                ws.Cell(row, 5).Value = r.DealCloseRate / 100;   // store as decimal for % format
                ws.Cell(row, 6).Value = r.TotalDealValueUSD;
                ws.Cell(row, 7).Value = r.CollectedRevenueUSD;
                ws.Cell(row, 8).Value = r.AvgDaysToClose;
                ws.Cell(row, 9).Value = r.ActiveClientsCount;
                ws.Cell(row, 10).Value = pipelineStage;
                ws.Cell(row, 11).Value = r.Email;

                for (int c = 1; c <= 11; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Fill.BackgroundColor = bg;
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Font.FontSize = 10;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = BorderColor;
                }

                // Specific formats
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.0%;-0.0%;\"-\"";
                ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0;($#,##0);\"-\"";
                ws.Cell(row, 7).Style.NumberFormat.Format = "$#,##0;($#,##0);\"-\"";
                ws.Cell(row, 8).Style.NumberFormat.Format = "0.0";
                ws.Cell(row, 7).Style.Font.FontColor = r.CollectedRevenueUSD > 0 ? PositiveFg : XLColor.Black;

                // Close rate conditional color
                ws.Cell(row, 5).Style.Font.FontColor = r.DealCloseRate >= 50
                    ? PositiveFg
                    : r.DealCloseRate >= 25
                        ? XLColor.FromHtml("#9C6500")
                        : NegativeFg;

                row++;
            }

            // Totals row
            if (data.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "TOTAL / AVG";
                ws.Cell(row, 3).FormulaA1 = $"SUM(C5:C{row - 1})";
                ws.Cell(row, 4).FormulaA1 = $"SUM(D5:D{row - 1})";
                ws.Cell(row, 5).FormulaA1 = $"IFERROR(D{row}/C{row},0)";
                ws.Cell(row, 6).FormulaA1 = $"SUM(F5:F{row - 1})";
                ws.Cell(row, 7).FormulaA1 = $"SUM(G5:G{row - 1})";
                ws.Cell(row, 8).FormulaA1 = $"IFERROR(AVERAGEIF(H5:H{row - 1},\">0\"),0)";
                ws.Cell(row, 9).FormulaA1 = $"SUM(I5:I{row - 1})";

                for (int c = 1; c <= 11; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Fill.BackgroundColor = TotalsBg;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                    cell.Style.Border.TopBorderColor = SubHeaderBg;
                }

                ws.Cell(row, 5).Style.NumberFormat.Format = "0.0%;-0.0%;\"-\"";
                ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0;($#,##0);\"-\"";
                ws.Cell(row, 7).Style.NumberFormat.Format = "$#,##0;($#,##0);\"-\"";
                ws.Cell(row, 8).Style.NumberFormat.Format = "0.0";
                ws.Row(row).Height = 18;
            }

            // Column widths
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 14;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 10;
            ws.Column(5).Width = 13;
            ws.Column(6).Width = 18;
            ws.Column(7).Width = 18;
            ws.Column(8).Width = 18;
            ws.Column(9).Width = 15;
            ws.Column(10).Width = 16;
            ws.Column(11).Width = 26;

            ws.SheetView.FreezeRows(4);
            ws.SheetView.FreezeColumns(1);
        }

        private void BuildPipelineBreakdownSheet(XLWorkbook wb, EmployeePerformanceDto data)
        {
            var ws = wb.Worksheets.Add("Pipeline Breakdown");
            ws.ShowGridLines = false;

            MergeAndStyle(ws, "A1:H1", "Deal Pipeline by Employee",
                14, bold: true, bg: SubHeaderBg, fg: XLColor.White);

            var headers = new[]
            {
            "Employee", "Lead", "Negotiation", "Confirmed", "Shipping", "Delivered", "Closed", "Total"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = SubHeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            ws.Row(3).Height = 20;

            int row = 4;
            foreach (var r in data.Rows)
            {
                bool isAlt = (row % 2 == 0);
                var bg = isAlt ? AlternateRow : XLColor.White;

                ws.Cell(row, 1).Value = r.FullName;
                ws.Cell(row, 2).Value = r.LeadCount;
                ws.Cell(row, 3).Value = r.NegotiationCount;
                ws.Cell(row, 4).Value = r.ConfirmedCount;
                ws.Cell(row, 5).Value = r.ShippingCount;
                ws.Cell(row, 6).Value = r.DeliveredCount;
                ws.Cell(row, 7).Value = r.ClosedCount;
                ws.Cell(row, 8).FormulaA1 = $"SUM(B{row}:G{row})";  // Excel formula — not hardcoded

                for (int c = 1; c <= 8; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Fill.BackgroundColor = bg;
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Font.FontSize = 10;
                    cell.Style.Alignment.Horizontal = c == 1
                        ? XLAlignmentHorizontalValues.Left
                        : XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = BorderColor;
                }

                // Highlight the closed column in green if > 0
                if (r.ClosedCount > 0)
                    ws.Cell(row, 7).Style.Font.FontColor = PositiveFg;

                row++;
            }

            // Totals
            if (data.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "TOTAL";
                for (int c = 2; c <= 8; c++)
                {
                    ws.Cell(row, c).FormulaA1 = $"SUM({GetColLetter(c)}4:{GetColLetter(c)}{row - 1})";
                    ws.Cell(row, c).Style.Font.Bold = true;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = TotalsBg;
                    ws.Cell(row, c).Style.Font.FontName = "Arial";
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, c).Style.Border.TopBorder = XLBorderStyleValues.Medium;
                    ws.Cell(row, c).Style.Border.TopBorderColor = SubHeaderBg;
                }

                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = TotalsBg;
                ws.Cell(row, 1).Style.Font.FontName = "Arial";
                ws.Cell(row, 1).Style.Border.TopBorder = XLBorderStyleValues.Medium;
                ws.Cell(row, 1).Style.Border.TopBorderColor = SubHeaderBg;
                ws.Row(row).Height = 18;
            }

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 22;
            ws.SheetView.FreezeRows(3);
            ws.SheetView.FreezeColumns(1);
        }

        // ── Shared helpers ─────────────────────────────────────────────────────────

        private static void MergeAndStyle(
            IXLWorksheet ws, string range, string text,
            int fontSize, bool bold,
            XLColor bg, XLColor fg)
        {
            ws.Range(range).Merge();
            var cell = ws.Range(range).FirstCell();
            cell.Value = text;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = fontSize;
            cell.Style.Font.Bold = bold;
            cell.Style.Font.FontColor = fg;
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Indent = 1;
        }

        private static void SetKpiCell(
            IXLWorksheet ws, string cellAddr, string label,
            object value, XLColor valueFg, bool isCurrency = true)
        {
            var labelCell = ws.Cell(cellAddr);
            var row = labelCell.Address.RowNumber;
            var col = labelCell.Address.ColumnNumber;

            labelCell.Value = label;
            labelCell.Style.Font.FontName = "Arial";
            labelCell.Style.Font.FontSize = 9;
            labelCell.Style.Font.FontColor = XLColor.FromHtml("#44546A");
            labelCell.Style.Font.Bold = false;

            var valueCell = ws.Cell(row + 1, col);
            valueCell.Value = ToCellValue(value);
            valueCell.Style.Font.FontName = "Arial";
            valueCell.Style.Font.FontSize = 15;
            valueCell.Style.Font.Bold = true;
            valueCell.Style.Font.FontColor = valueFg;

            if (isCurrency)
                valueCell.Style.NumberFormat.Format = "$#,##0;($#,##0);\"-\"";
            else
                valueCell.Style.NumberFormat.Format = "#,##0";
        }

        private static void SetDataCell(
            IXLWorksheet ws, int row, int col,
            object value, XLColor bg, bool isText)
        {
            var cell = ws.Cell(row, col);
            cell.Value = ToCellValue(value);
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = 10;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = BorderColor;

            if (!isText)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.NumberFormat.Format = "#,##0";
            }
        }

        private static void SetCurrencyCell(
            IXLWorksheet ws, int row, int col,
            decimal value, XLColor bg, XLColor fg)
        {
            var cell = ws.Cell(row, col);
            cell.Value = value;
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.FontColor = fg;
            cell.Style.NumberFormat.Format = "$#,##0.00;($#,##0.00);\"-\"";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = BorderColor;
        }

        private static string GetDominantStage(EmployeePerformanceRowDto r)
        {
            var stages = new[]
            {
            (r.LeadCount,        "Lead"),
            (r.NegotiationCount, "Negotiation"),
            (r.ConfirmedCount,   "Confirmed"),
            (r.ShippingCount,    "Shipping"),
            (r.DeliveredCount,   "Delivered"),
            (r.ClosedCount,      "Closed"),
        };

            var dominant = stages.OrderByDescending(s => s.Item1).First();
            return dominant.Item1 > 0 ? dominant.Item2 : "—";
        }

        private static string GetColLetter(int col)
        {
            return col switch
            {
                1 => "A",
                2 => "B",
                3 => "C",
                4 => "D",
                5 => "E",
                6 => "F",
                7 => "G",
                8 => "H",
                _ => ((char)('A' + col - 1)).ToString()
            };
        }

        private static XLCellValue ToCellValue(object? value)
        {
            return value switch
            {
                null => Blank.Value,
                XLCellValue cellValue => cellValue,
                string s => s,
                int i => i,
                long l => l,
                short sh => sh,
                uint ui => ui,
                ulong ul => ul,
                decimal d => d,
                double db => db,
                float f => f,
                bool b => b,
                DateTime dt => dt,
                DateTimeOffset dto => dto.DateTime,
                TimeSpan ts => ts,
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
