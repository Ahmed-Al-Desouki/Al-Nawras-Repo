using Al_Nawras.Application.Reports.Queries.ExportReport;
using Al_Nawras.Application.Reports.Queries.GetEmployeePerformance;
using Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Policy = "InternalOnly")]
    public class ReportsController : ControllerBase
    {
        private readonly GetRevenueByPeriodHandler _revenueHandler;
        private readonly GetEmployeePerformanceHandler _performanceHandler;
        private readonly ExportReportHandler _exportHandler;

        public ReportsController(
            GetRevenueByPeriodHandler revenueHandler,
            GetEmployeePerformanceHandler performanceHandler,
            ExportReportHandler exportHandler)
        {
            _revenueHandler = revenueHandler;
            _performanceHandler = performanceHandler;
            _exportHandler = exportHandler;
        }

        // ── JSON endpoints ─────────────────────────────────────────────────────────

        /// <summary>Revenue breakdown by period — returns JSON for dashboard charts</summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] ReportGrouping grouping = ReportGrouping.Monthly,
            CancellationToken cancellationToken = default)
        {
            var result = await _revenueHandler.Handle(
                new GetRevenueByPeriodQuery(from, to, grouping), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>Employee performance metrics — returns JSON for dashboard tables</summary>
        [HttpGet("employee-performance")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetEmployeePerformance(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _performanceHandler.Handle(
                new GetEmployeePerformanceQuery(from, to, userId), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        // ── Excel export endpoints ─────────────────────────────────────────────────

        /// <summary>Export revenue report to Excel (.xlsx)</summary>
        [HttpGet("revenue/export")]
        public async Task<IActionResult> ExportRevenue(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] ReportGrouping grouping = ReportGrouping.Monthly,
            CancellationToken cancellationToken = default)
        {
            var result = await _exportHandler.Handle(
                new ExportReportQuery(ReportType.Revenue, from, to, grouping),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return File(result.Value.Bytes, result.Value.ContentType, result.Value.FileName);
        }

        /// <summary>Export employee performance report to Excel (.xlsx)</summary>
        [HttpGet("employee-performance/export")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ExportEmployeePerformance(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _exportHandler.Handle(
                new ExportReportQuery(ReportType.EmployeePerformance, from, to, UserId: userId),
                cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return File(result.Value.Bytes, result.Value.ContentType, result.Value.FileName);
        }
    }
}
