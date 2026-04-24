using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyDeals
{
    public record GetMyDealsQuery(
       Guid ClientId,
       DealStatus? StatusFilter = null
   );

}
