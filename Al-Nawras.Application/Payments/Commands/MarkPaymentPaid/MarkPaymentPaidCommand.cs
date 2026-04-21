using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Payments.Commands.MarkPaymentPaid
{
    public record MarkPaymentPaidCommand(Guid PaymentId);
}
