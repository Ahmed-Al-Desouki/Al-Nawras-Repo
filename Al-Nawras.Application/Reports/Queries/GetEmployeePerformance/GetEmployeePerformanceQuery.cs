using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.Queries.GetEmployeePerformance
{
    public record GetEmployeePerformanceQuery(
        DateTime PeriodStart,
        DateTime PeriodEnd,
        int? UserId = null   // null = all employees
    );
}
