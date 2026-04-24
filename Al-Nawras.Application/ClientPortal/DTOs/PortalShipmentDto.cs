using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.DTOs
{
    public record PortalShipmentDto(
        Guid Id,
        Guid DealId,
        string ShipmentNumber,
        string Status,
        string? TrackingNumber,
        string? Carrier,
        string? VesselName,
        string? PortOfLoading,
        string? PortOfDischarge,
        DateTime? ETD,
        DateTime? ETA,
        DateTime? ActualDeparture,
        DateTime? ActualArrival
    );
}
