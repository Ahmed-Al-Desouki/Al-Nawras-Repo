using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Shipments.DTOs
{
    public record ShipmentDto(
        Guid Id,
        Guid DealId,
        string DealNumber,
        string ShipmentNumber,
        ShipmentStatus Status,
        string StatusLabel,
        string TrackingNumber,
        string Carrier,
        string VesselName,
        string PortOfLoading,
        string PortOfDischarge,
        DateTime? ETD,
        DateTime? ETA,
        DateTime? ActualDeparture,
        DateTime? ActualArrival,
        DateTime CreatedAt
    );
}
