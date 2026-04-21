using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus
{
    public record UpdateShipmentStatusCommand(
        Guid ShipmentId,
        ShipmentStatus NewStatus,
        string? TrackingNumber
    );
}
