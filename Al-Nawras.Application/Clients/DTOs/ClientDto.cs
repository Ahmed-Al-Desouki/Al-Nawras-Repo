using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Clients.DTOs
{
    public record ClientDto(
        Guid Id,
        string Name,
        string Email,
        string Phone,
        string Country,
        string CompanyName,
        int? AssignedSalesUserId,
        DateTime CreatedAt
    );
}
