using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod
{
    public enum ReportGrouping { Monthly, Quarterly, Yearly }

    public record GetRevenueByPeriodQuery(
        DateTime PeriodStart,
        DateTime PeriodEnd,
        ReportGrouping Grouping = ReportGrouping.Monthly
    );
}
