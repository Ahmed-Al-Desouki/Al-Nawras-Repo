using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Shipments.Commands.CreateShipment
{

    public record CreateShipmentCommand(
        Guid DealId,
        string Carrier,
        string PortOfLoading,
        string PortOfDischarge,
        DateTime? ETD,
        DateTime? ETA
    );
}
