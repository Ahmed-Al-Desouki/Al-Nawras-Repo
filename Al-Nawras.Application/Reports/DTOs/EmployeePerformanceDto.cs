using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.DTOs
{
    public record EmployeePerformanceDto(
        List<EmployeePerformanceRowDto> Rows,
        DateTime PeriodStart,
        DateTime PeriodEnd
    );

    public record EmployeePerformanceRowDto(
        int UserId,
        string FullName,
        string Email,
        string Role,

        // Deal pipeline
        int TotalDeals,
        int LeadCount,
        int NegotiationCount,
        int ConfirmedCount,
        int ShippingCount,
        int DeliveredCount,
        int ClosedCount,

        // Financial
        decimal TotalDealValueUSD,
        decimal CollectedRevenueUSD,

        // Performance metrics
        double DealCloseRate,       // ClosedCount / TotalDeals
        double AvgDaysToClose,      // avg days from CreatedAt to ClosedAt
        int ActiveClientsCount
    );
}
