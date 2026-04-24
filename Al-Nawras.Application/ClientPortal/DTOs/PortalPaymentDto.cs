using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.DTOs
{
    public record PortalPaymentDto(
        Guid Id,
        string PaymentReference,
        decimal Amount,
        string Currency,
        string Status,
        string PaymentType,
        DateTime DueDate,
        DateTime? PaidAt
    );
}
