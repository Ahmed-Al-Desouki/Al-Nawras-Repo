using System.Globalization;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Reporting.DTOs;
using ClosedXML.Excel;

namespace Al_Nawras.Infrastructure.Services
{
    public class ReportingWorkbookService : IReportingWorkbookService
    {
        private const int DataHeaderRow = 6;
        private const int FirstDataRow = 7;
        private const int TemplateSampleRows = 25;
        private static readonly XLColor HeaderBg = XLColor.FromHtml("#1F3864");
        private static readonly XLColor HeaderFg = XLColor.White;
        private static readonly XLColor SubHeaderBg = XLColor.FromHtml("#2E75B6");
        private static readonly XLColor AlternateRow = XLColor.FromHtml("#F2F7FB");
        private static readonly XLColor TotalsBg = XLColor.FromHtml("#D6E4F0");
        private static readonly XLColor SoftBg = XLColor.FromHtml("#EAF1F7");

        public byte[] GenerateTemplateWorkbook(ReportTemplateDefinitionDto definition)
        {
            using var workbook = new XLWorkbook();

            BuildInstructionsSheet(workbook, definition);

            foreach (var sheetDefinition in definition.Sheets)
            {
                BuildTemplateSheet(workbook, definition, sheetDefinition);
                BuildAnalysisSheet(workbook, sheetDefinition);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public ImportedWorkbookParseResult ParseWorkbook(Stream fileStream, string fileName)
        {
            fileStream.Position = 0;

            using var workbook = new XLWorkbook(fileStream);
            var worksheets = new List<WorkbookSheetSnapshotDto>();
            var headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var numericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dateColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var totalRows = 0;
            var totalNonEmptyCells = 0;

            foreach (var worksheet in workbook.Worksheets)
            {
                var range = worksheet.RangeUsed();
                if (range is null)
                {
                    worksheets.Add(new WorkbookSheetSnapshotDto(worksheet.Name, 0, 0, [], []));
                    continue;
                }

                var rows = new List<List<string>>();
                var headerRow = new List<string>();
                var columnCount = range.ColumnCount();
                var rowCount = range.RowCount();

                for (var rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                {
                    var rowValues = new List<string>();

                    for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
                    {
                        var cellValue = worksheet.Cell(
                                range.FirstRow().RowNumber() + rowIndex - 1,
                                range.FirstColumn().ColumnNumber() + columnIndex - 1)
                            .GetFormattedString()
                            .Trim();

                        rowValues.Add(cellValue);

                        if (!string.IsNullOrWhiteSpace(cellValue))
                            totalNonEmptyCells++;

                        if (rowIndex == 1)
                        {
                            headerRow.Add(cellValue);
                            if (!string.IsNullOrWhiteSpace(cellValue))
                                headers.Add(cellValue);
                        }
                    }

                    rows.Add(rowValues);
                }

                DetectTypedColumns(rows, headerRow, numericColumns, dateColumns);

                totalRows += Math.Max(rowCount - 1, 0);
                worksheets.Add(new WorkbookSheetSnapshotDto(
                    worksheet.Name,
                    Math.Max(rowCount - 1, 0),
                    columnCount,
                    headerRow,
                    rows));
            }

            return new ImportedWorkbookParseResult(
                new WorkbookSnapshotDto(fileName, worksheets.Count, totalRows, totalNonEmptyCells, worksheets),
                new ImportedWorkbookAnalysisDto(
                    headers.OrderBy(h => h).ToList(),
                    numericColumns.OrderBy(h => h).ToList(),
                    dateColumns.OrderBy(h => h).ToList(),
                    new ImportLinkAnalysisDto([], [], [], [])));
        }

        private static void BuildInstructionsSheet(XLWorkbook workbook, ReportTemplateDefinitionDto definition)
        {
            var worksheet = workbook.Worksheets.Add("Instructions");
            worksheet.ShowGridLines = false;

            worksheet.Cell("A1").Value = definition.Name;
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.White;
            worksheet.Range("A1:H1").Merge().Style.Fill.BackgroundColor = XLColor.FromHtml("#17324D");

            worksheet.Cell("A2").Value = definition.Description;
            worksheet.Range("A2:H2").Merge().Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1F7");
            worksheet.Cell("A2").Style.Font.Italic = true;

            worksheet.Cell("A4").Value = "How To Use";
            worksheet.Cell("A4").Style.Font.Bold = true;
            worksheet.Cell("A4").Style.Font.FontSize = 13;

            var instructions = new[]
            {
                "1. Use the data-entry sheets only for business records.",
                "2. Keep the header row unchanged so imports and analysis continue to work.",
                "3. Add new records inside the blue Excel table; filters and formulas expand automatically.",
                "4. Use the totals row at the bottom of each table for quick SUM / AVG / COUNT calculations.",
                "5. Use the Analysis sheets for ready-made formulas by numeric/date column.",
                "6. Optional guidance and examples are listed below for every sheet."
            };

            for (var i = 0; i < instructions.Length; i++)
            {
                worksheet.Cell(5 + i, 1).Value = instructions[i];
            }

            var row = 13;
            foreach (var sheet in definition.Sheets)
            {
                worksheet.Cell(row, 1).Value = sheet.SheetName;
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#2C7DA0");
                row++;

                worksheet.Cell(row, 1).Value = "Purpose";
                worksheet.Cell(row, 2).Value = sheet.Purpose;
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F7FBFD");
                row++;

                worksheet.Cell(row, 1).Value = "Column";
                worksheet.Cell(row, 2).Value = "Type";
                worksheet.Cell(row, 3).Value = "Required";
                worksheet.Cell(row, 4).Value = "Example";
                worksheet.Cell(row, 5).Value = "Notes";
                worksheet.Range(row, 1, row, 5).Style.Font.Bold = true;
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF4");
                row++;

                foreach (var column in sheet.Columns)
                {
                    worksheet.Cell(row, 1).Value = column.Header;
                    worksheet.Cell(row, 2).Value = column.DataType;
                    worksheet.Cell(row, 3).Value = column.IsRequired ? "Yes" : "No";
                    worksheet.Cell(row, 4).Value = column.ExampleValue;
                    worksheet.Cell(row, 5).Value = column.Notes;
                    row++;
                }

                row += 2;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 22);
            worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 28);
            worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 32);
            worksheet.SheetView.FreezeRows(4);
        }

        private static void BuildTemplateSheet(
            XLWorkbook workbook,
            ReportTemplateDefinitionDto definition,
            ReportTemplateSheetDefinitionDto sheetDefinition)
        {
            var worksheet = workbook.Worksheets.Add(sheetDefinition.SheetName);
            worksheet.ShowGridLines = false;

            var lastColumn = Math.Max(sheetDefinition.Columns.Count, 1);

            worksheet.Cell(1, 1).Value = definition.Name;
            worksheet.Range(1, 1, 1, lastColumn).Merge();
            worksheet.Range(1, 1, 1, lastColumn).Style.Fill.BackgroundColor = HeaderBg;
            worksheet.Range(1, 1, 1, lastColumn).Style.Font.FontColor = HeaderFg;
            worksheet.Range(1, 1, 1, lastColumn).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, lastColumn).Style.Font.FontSize = 18;

            worksheet.Cell(2, 1).Value = $"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC";
            worksheet.Range(2, 1, 2, lastColumn).Merge();
            worksheet.Range(2, 1, 2, lastColumn).Style.Fill.BackgroundColor = HeaderBg;
            worksheet.Range(2, 1, 2, lastColumn).Style.Font.FontColor = XLColor.FromHtml("#BDD7EE");

            worksheet.Cell(3, 1).Value = sheetDefinition.Purpose;
            worksheet.Range(3, 1, 3, lastColumn).Merge();
            worksheet.Range(3, 1, 3, lastColumn).Style.Fill.BackgroundColor = SoftBg;
            worksheet.Range(3, 1, 3, lastColumn).Style.Font.Italic = true;

            worksheet.Cell(4, 1).Value = "Data Entry Sheet";
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml("#1D4E6D");

            worksheet.Cell(5, 1).Value = "Tip:";
            worksheet.Cell(5, 2).Value = "Add rows directly inside the table below. Filters, totals, and analysis formulas stay organized automatically.";
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Range(5, 1, 5, lastColumn).Style.Fill.BackgroundColor = XLColor.FromHtml("#F7FBFD");

            for (var index = 0; index < sheetDefinition.Columns.Count; index++)
            {
                var column = sheetDefinition.Columns[index];
                worksheet.Cell(DataHeaderRow, index + 1).Value = column.Header;
            }

            for (var row = FirstDataRow; row < FirstDataRow + TemplateSampleRows; row++)
            {
                for (var columnIndex = 0; columnIndex < sheetDefinition.Columns.Count; columnIndex++)
                {
                    var column = sheetDefinition.Columns[columnIndex];
                    worksheet.Cell(row, columnIndex + 1).Value = row == FirstDataRow
                        ? column.ExampleValue
                        : string.Empty;
                }
            }

            var tableRange = worksheet.Range(
                DataHeaderRow,
                1,
                FirstDataRow + TemplateSampleRows - 1,
                lastColumn);

            var tableName = $"tbl{SanitizeName(sheetDefinition.SheetName)}";
            var table = tableRange.CreateTable(tableName);
            table.Theme = XLTableTheme.None;
            table.SetShowAutoFilter(true);
            table.SetShowTotalsRow(true);
            table.SetShowHeaderRow(true);
            table.SetShowRowStripes(true);

            for (var index = 0; index < sheetDefinition.Columns.Count; index++)
            {
                var field = table.Field(index);
                var definitionColumn = sheetDefinition.Columns[index];

                if (IsNumericColumn(definitionColumn))
                {
                    field.TotalsRowFunction = XLTotalsRowFunction.Sum;
                    worksheet.Column(index + 1).Style.NumberFormat.Format = "#,##0.00";
                }
                else
                {
                    field.TotalsRowLabel = index == 0 ? "Totals / Quick Calc" : string.Empty;
                }

                ApplyColumnFormatting(worksheet.Column(index + 1), definitionColumn);
                AddComment(worksheet.Cell(DataHeaderRow, index + 1), definitionColumn);
            }

            for (var index = 1; index <= lastColumn; index++)
            {
                var headerCell = worksheet.Cell(DataHeaderRow, index);
                headerCell.Style.Fill.BackgroundColor = SubHeaderBg;
                headerCell.Style.Font.FontColor = HeaderFg;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headerCell.Style.Border.BottomBorderColor = HeaderBg;
            }

            var totalsRowNumber = FirstDataRow + TemplateSampleRows - 1;
            for (var index = 1; index <= lastColumn; index++)
            {
                var totalsCell = worksheet.Cell(totalsRowNumber, index);
                totalsCell.Style.Fill.BackgroundColor = TotalsBg;
                totalsCell.Style.Font.Bold = true;
                totalsCell.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                totalsCell.Style.Border.TopBorderColor = SubHeaderBg;
            }

            for (var row = FirstDataRow; row < totalsRowNumber; row++)
            {
                if ((row - FirstDataRow) % 2 != 0)
                {
                    worksheet.Range(row, 1, row, lastColumn).Style.Fill.BackgroundColor = AlternateRow;
                }
            }

            worksheet.Range(DataHeaderRow, 1, FirstDataRow + TemplateSampleRows - 1, lastColumn)
                .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Range(DataHeaderRow, 1, FirstDataRow + TemplateSampleRows - 1, lastColumn)
                .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(DataHeaderRow, 1, FirstDataRow + TemplateSampleRows - 1, lastColumn)
                .Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            worksheet.SheetView.FreezeRows(DataHeaderRow);
            worksheet.Columns().AdjustToContents();
            worksheet.Row(1).Height = 28;
            worksheet.Row(2).Height = 18;
            worksheet.Row(3).Height = 22;
            worksheet.Row(DataHeaderRow).Height = 20;

            for (var index = 0; index < sheetDefinition.Columns.Count; index++)
            {
                worksheet.Column(index + 1).Width = Math.Min(Math.Max(worksheet.Column(index + 1).Width + 2, 15), 28);
            }
        }

        private static void BuildAnalysisSheet(
            XLWorkbook workbook,
            ReportTemplateSheetDefinitionDto sheetDefinition)
        {
            var worksheet = workbook.Worksheets.Add($"{TrimSheetName(sheetDefinition.SheetName)} Analysis");
            worksheet.ShowGridLines = false;

            var tableName = $"tbl{SanitizeName(sheetDefinition.SheetName)}";
            var numericColumns = sheetDefinition.Columns.Where(IsNumericColumn).ToList();
            var dateColumns = sheetDefinition.Columns.Where(IsDateColumn).ToList();

            worksheet.Cell("A1").Value = $"{sheetDefinition.SheetName} Quick Analysis";
            worksheet.Range("A1:F1").Merge().Style.Fill.BackgroundColor = XLColor.FromHtml("#17324D");
            worksheet.Range("A1:F1").Style.Font.FontColor = XLColor.White;
            worksheet.Range("A1:F1").Style.Font.Bold = true;
            worksheet.Range("A1:F1").Style.Font.FontSize = 16;

            worksheet.Cell("A2").Value = "This sheet gives ready-made Excel formulas so totals and analysis stay easy and consistent.";
            worksheet.Range("A2:F2").Merge().Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1F7");

            worksheet.Cell("A4").Value = "Numeric Columns";
            worksheet.Cell("A4").Style.Font.Bold = true;

            worksheet.Cell("A5").Value = "Column";
            worksheet.Cell("B5").Value = "SUM";
            worksheet.Cell("C5").Value = "AVERAGE";
            worksheet.Cell("D5").Value = "MIN";
            worksheet.Cell("E5").Value = "MAX";
            worksheet.Cell("F5").Value = "COUNT";
            worksheet.Range("A5:F5").Style.Font.Bold = true;
            worksheet.Range("A5:F5").Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF4");

            var row = 6;
            foreach (var column in numericColumns)
            {
                var columnReference = EscapeStructuredReference(column.Header);
                worksheet.Cell(row, 1).Value = column.Header;
                worksheet.Cell(row, 2).FormulaA1 = $"=SUBTOTAL(109,{tableName}[{columnReference}])";
                worksheet.Cell(row, 3).FormulaA1 = $"=IFERROR(SUBTOTAL(101,{tableName}[{columnReference}]),0)";
                worksheet.Cell(row, 4).FormulaA1 = $"=IFERROR(MIN({tableName}[{columnReference}]),0)";
                worksheet.Cell(row, 5).FormulaA1 = $"=IFERROR(MAX({tableName}[{columnReference}]),0)";
                worksheet.Cell(row, 6).FormulaA1 = $"=COUNT({tableName}[{columnReference}])";
                worksheet.Range(row, 2, row, 6).Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            row += 2;
            worksheet.Cell(row, 1).Value = "Date Columns";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            worksheet.Cell(row, 1).Value = "Column";
            worksheet.Cell(row, 2).Value = "Earliest";
            worksheet.Cell(row, 3).Value = "Latest";
            worksheet.Cell(row, 4).Value = "Count";
            worksheet.Range(row, 1, row, 4).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF4");
            row++;

            foreach (var column in dateColumns)
            {
                var columnReference = EscapeStructuredReference(column.Header);
                worksheet.Cell(row, 1).Value = column.Header;
                worksheet.Cell(row, 2).FormulaA1 = $"=IFERROR(MIN({tableName}[{columnReference}]),\"\")";
                worksheet.Cell(row, 3).FormulaA1 = $"=IFERROR(MAX({tableName}[{columnReference}]),\"\")";
                worksheet.Cell(row, 4).FormulaA1 = $"=COUNTA({tableName}[{columnReference}])";
                worksheet.Range(row, 2, row, 3).Style.DateFormat.Format = "yyyy-mm-dd";
                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(5);
        }

        private static void ApplyColumnFormatting(IXLColumn column, ReportTemplateColumnDefinitionDto definitionColumn)
        {
            if (IsDateColumn(definitionColumn))
            {
                column.Style.DateFormat.Format = "yyyy-mm-dd";
                return;
            }

            if (IsNumericColumn(definitionColumn))
            {
                column.Style.NumberFormat.Format = "#,##0.00";
                return;
            }

            column.Style.NumberFormat.Format = "@";
        }

        private static void AddComment(IXLCell cell, ReportTemplateColumnDefinitionDto definitionColumn)
        {
            var comment = cell.GetComment();
            comment.ClearText();
            comment.AddText($"Type: {definitionColumn.DataType}");
            comment.AddNewLine();
            comment.AddText($"Required: {(definitionColumn.IsRequired ? "Yes" : "No")}");
            comment.AddNewLine();
            comment.AddText($"Example: {definitionColumn.ExampleValue}");

            if (!string.IsNullOrWhiteSpace(definitionColumn.Notes))
            {
                comment.AddNewLine();
                comment.AddText($"Notes: {definitionColumn.Notes}");
            }
        }

        private static bool IsNumericColumn(ReportTemplateColumnDefinitionDto column)
            => string.Equals(column.DataType, "Number", StringComparison.OrdinalIgnoreCase);

        private static bool IsDateColumn(ReportTemplateColumnDefinitionDto column)
            => string.Equals(column.DataType, "Date", StringComparison.OrdinalIgnoreCase);

        private static string SanitizeName(string value)
        {
            var chars = value.Where(char.IsLetterOrDigit).ToArray();
            var sanitized = new string(chars);
            return string.IsNullOrWhiteSpace(sanitized) ? "Sheet" : sanitized;
        }

        private static string EscapeStructuredReference(string value)
            => value.Replace("]", "]]", StringComparison.Ordinal);

        private static string TrimSheetName(string value)
            => value.Length <= 24 ? value : value[..24];

        private static void DetectTypedColumns(
            List<List<string>> rows,
            List<string> headers,
            HashSet<string> numericColumns,
            HashSet<string> dateColumns)
        {
            if (rows.Count <= 1 || headers.Count == 0)
                return;

            for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                var values = rows.Skip(1)
                    .Where(r => columnIndex < r.Count)
                    .Select(r => r[columnIndex])
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Take(25)
                    .ToList();

                if (values.Count == 0)
                    continue;

                if (values.All(v => decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                                 || decimal.TryParse(v, NumberStyles.Any, CultureInfo.CurrentCulture, out _)))
                {
                    numericColumns.Add(headers[columnIndex]);
                }

                if (values.All(v => DateTime.TryParse(v, out _)))
                    dateColumns.Add(headers[columnIndex]);
            }
        }
    }
}
