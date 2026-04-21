using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Enums
{
    public enum ShipmentStatus
    {
        Pending = 0,
        BookingConfirmed = 1,
        InTransit = 2,
        AtCustoms = 3,
        Delivered = 4
    }
}
