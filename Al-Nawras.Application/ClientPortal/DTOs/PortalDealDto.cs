using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.DTOs
{
    public record PortalDealDto(
        Guid Id,
        string DealNumber,
        string Status,
        string Commodity,
        decimal TotalValue,
        string Currency,
        string Origin,
        string Destination,
        DateTime? ConfirmedAt,
        DateTime CreatedAt,
        int ShipmentCount,
        int PaidPaymentsCount,
        int TotalPaymentsCount
    );

    public record PortalDealDetailDto(
        Guid Id,
        string DealNumber,
        string Status,
        string Commodity,
        decimal TotalValue,
        string Currency,
        string Origin,
        string Destination,
        DateTime? ConfirmedAt,
        DateTime CreatedAt,
        List<PortalShipmentDto> Shipments,
        List<PortalPaymentDto> Payments,
        List<PortalDocumentDto> Documents,
        List<PortalStatusEventDto> StatusHistory
    );

    public record PortalStatusEventDto(
        string FromStatus,
        string ToStatus,
        DateTime ChangedAt
    );
}
