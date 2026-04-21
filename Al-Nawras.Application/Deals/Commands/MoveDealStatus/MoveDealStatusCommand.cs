using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Commands.MoveDealStatus
{
    public record MoveDealStatusCommand(
        Guid DealId,
        DealStatus NewStatus,
        int ChangedByUserId,
        string Notes
    );
}
