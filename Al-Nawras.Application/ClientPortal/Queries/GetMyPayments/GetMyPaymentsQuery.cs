using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyPayments
{
    public record GetMyPaymentsQuery(
        Guid ClientId,
        PaymentStatus? StatusFilter = null
    );
}
