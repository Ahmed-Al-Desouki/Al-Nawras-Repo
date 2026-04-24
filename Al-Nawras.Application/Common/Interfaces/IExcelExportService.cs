using Al_Nawras.Application.Reports.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces
{
    public interface IExcelExportService
    {
        byte[] ExportRevenuByPeriod(RevenueByPeriodDto data, DateTime from, DateTime to, string grouping);
        byte[] ExportEmployeePerformance(EmployeePerformanceDto data);
    }
}
