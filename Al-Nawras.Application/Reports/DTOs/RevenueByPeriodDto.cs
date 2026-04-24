using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.DTOs
{
    public record RevenueByPeriodDto(
        // Summary totals
        decimal TotalRevenueUSD,
        decimal TotalPendingUSD,
        decimal TotalOverdueUSD,
        int TotalDeals,
        int TotalPaidPayments,

        // Breakdown rows
        List<RevenuePeriodRowDto> Rows
    );

    public record RevenuePeriodRowDto(
        string PeriodLabel,        // "Jan 2025" / "Q1 2025" / "2025"
        int Year,
        int? Month,              // null for quarterly/yearly
        int? Quarter,            // null for monthly/yearly
        decimal CollectedUSD,       // fully paid payments
        decimal PendingUSD,         // not yet due
        decimal OverdueUSD,         // past due, not paid
        int DealCount,
        int PaymentCount,
        decimal AvgDealValueUSD
    );
}
