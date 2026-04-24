using Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.Queries.ExportReport
{
    public enum ReportType { Revenue, EmployeePerformance }

    public record ExportReportQuery(
        ReportType Type,
        DateTime PeriodStart,
        DateTime PeriodEnd,
        ReportGrouping Grouping = ReportGrouping.Monthly,
        int? UserId = null
    );
}
