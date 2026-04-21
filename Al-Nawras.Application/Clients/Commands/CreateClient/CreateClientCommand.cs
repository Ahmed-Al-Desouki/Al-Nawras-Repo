using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Clients.Commands.CreateClient
{
    public record CreateClientCommand(
        string Name,
        string Email,
        string Phone,
        string Country,
        string CompanyName,
        int? AssignedSalesUserId
    );
}
