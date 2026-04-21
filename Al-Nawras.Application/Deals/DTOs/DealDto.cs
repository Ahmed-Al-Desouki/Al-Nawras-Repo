using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.DTOs
{
    public record DealDto(
        Guid Id,
        string DealNumber,
        Guid ClientId,
        string ClientName,
        DealStatus Status,
        string StatusLabel,
        string Commodity,
        decimal TotalValue,
        string Currency,
        int AssignedSalesUserId,
        string AssignedSalesUserName,
        string Origin,
        string Destination,
        string Notes,
        DateTime? ConfirmedAt,
        DateTime? ClosedAt,
        DateTime CreatedAt
    );

    public record DealSummaryDto(
        Guid Id,
        string DealNumber,
        string ClientName,
        DealStatus Status,
        string StatusLabel,
        string Commodity,
        decimal TotalValue,
        string Currency,
        string Origin,
        string Destination,
        DateTime CreatedAt
    );
}
