using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Queries.GetDeals
{
    public record GetDealsQuery(
        DealStatus? Status = null,
        Guid? ClientId = null,
        int? AssignedUserId = null
    );
}
