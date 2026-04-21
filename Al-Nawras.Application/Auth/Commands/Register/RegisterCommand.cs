using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Auth.Commands.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        int RoleId = 2   // defaults to Sales — Admin manually assigns final role
    );
}
