using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Payments.Commands.CreatePayment
{
    public record CreatePaymentCommand(
        Guid DealId,
        decimal Amount,
        string Currency,
        PaymentType PaymentType,
        DateTime DueDate,
        string? Notes
    );
}
