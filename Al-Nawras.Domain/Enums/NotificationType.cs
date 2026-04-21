using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Enums
{
    public enum NotificationType
    {
        DealStatusChanged = 0,
        PaymentOverdue = 1,
        ShipmentDelayed = 2,
        DocumentUploaded = 3,
        TaskAssigned = 4
    }
}
