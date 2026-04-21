using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Dashboard.DTOs
{
    public record DashboardDto(
        // Deal stats
        int TotalActiveDeals,
        int LeadCount,
        int NegotiationCount,
        int ConfirmedCount,
        int ShippingCount,
        int CustomsCount,

        // Shipment stats
        int TotalShipmentsInTransit,
        int DelayedShipmentsCount,     // ETA passed, not yet delivered

        // Payment stats
        decimal TotalRevenueUSD,
        decimal OverduePaymentsUSD,
        int OverduePaymentsCount,

        // Recent activity
        List<RecentDealDto> RecentDeals,
        List<OverduePaymentDto> OverduePayments,
        List<DelayedShipmentDto> DelayedShipments
    );

    public record RecentDealDto(
        Guid Id,
        string DealNumber,
        string ClientName,
        string Status,
        decimal TotalValue,
        string Currency,
        DateTime CreatedAt
    );

    public record OverduePaymentDto(
        Guid Id,
        string PaymentReference,
        string DealNumber,
        string ClientName,
        decimal Amount,
        string Currency,
        decimal AmountUSD,
        DateTime DueDate,
        int DaysOverdue
    );

    public record DelayedShipmentDto(
        Guid Id,
        string ShipmentNumber,
        string DealNumber,
        string Carrier,
        string PortOfDischarge,
        DateTime? ETA,
        int DaysDelayed
    );
}
