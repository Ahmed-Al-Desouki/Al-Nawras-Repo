using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reports.Queries.GetEmployeePerformance;
using Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.Queries.ExportReport
{
    public record ExcelFileResult(byte[] Bytes, string FileName, string ContentType);

    public class ExportReportHandler
    {
        private readonly GetRevenueByPeriodHandler _revenueHandler;
        private readonly GetEmployeePerformanceHandler _performanceHandler;
        private readonly IExcelExportService _excelService;

        public ExportReportHandler(
            GetRevenueByPeriodHandler revenueHandler,
            GetEmployeePerformanceHandler performanceHandler,
            IExcelExportService excelService)
        {
            _revenueHandler = revenueHandler;
            _performanceHandler = performanceHandler;
            _excelService = excelService;
        }

        public async Task<Result<ExcelFileResult>> Handle(
            ExportReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");

            switch (query.Type)
            {
                case ReportType.Revenue:
                    {
                        var dataResult = await _revenueHandler.Handle(
                            new GetRevenueByPeriodQuery(query.PeriodStart, query.PeriodEnd, query.Grouping),
                            cancellationToken);

                        if (!dataResult.IsSuccess)
                            return Result<ExcelFileResult>.Failure(dataResult.Error);

                        var bytes = _excelService.ExportRevenuByPeriod(
                            dataResult.Value,
                            query.PeriodStart,
                            query.PeriodEnd,
                            query.Grouping.ToString());

                        var fileName = $"Revenue-Report-{timestamp}.xlsx";

                        return Result<ExcelFileResult>.Success(
                            new ExcelFileResult(bytes, fileName,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                    }

                case ReportType.EmployeePerformance:
                    {
                        var dataResult = await _performanceHandler.Handle(
                            new GetEmployeePerformanceQuery(query.PeriodStart, query.PeriodEnd, query.UserId),
                            cancellationToken);

                        if (!dataResult.IsSuccess)
                            return Result<ExcelFileResult>.Failure(dataResult.Error);

                        var bytes = _excelService.ExportEmployeePerformance(dataResult.Value);
                        var fileName = $"Employee-Performance-{timestamp}.xlsx";

                        return Result<ExcelFileResult>.Success(
                            new ExcelFileResult(bytes, fileName,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                    }

                default:
                    return Result<ExcelFileResult>.Failure($"Unknown report type: {query.Type}");
            }
        }
    }
}
