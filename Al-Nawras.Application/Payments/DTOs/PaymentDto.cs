using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Payments.DTOs
{
    public record PaymentDto(
        Guid Id,
        Guid DealId,
        string DealNumber,
        string PaymentReference,
        decimal Amount,
        string Currency,
        decimal ExchangeRateToUSD,
        decimal AmountUSD,
        PaymentStatus Status,
        string StatusLabel,
        PaymentType PaymentType,
        string PaymentTypeLabel,
        DateTime DueDate,
        DateTime? PaidAt,
        string Notes,
        DateTime CreatedAt
    );
}
