using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Commands.CreateDeal
{
    public record CreateDealCommand(
        Guid ClientId,
        string Commodity,
        decimal TotalValue,
        string Currency,
        int AssignedSalesUserId,
        string Origin,
        string Destination,
        string Notes
    );
}
