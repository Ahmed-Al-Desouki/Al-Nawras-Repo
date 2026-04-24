using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyDealDetail
{
    public record GetMyDealDetailQuery(Guid DealId, Guid ClientId);
}
