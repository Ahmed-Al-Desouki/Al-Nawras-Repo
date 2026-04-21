using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        PartiallyPaid = 1,
        FullyPaid = 2,
        Overdue = 3
    }
}
