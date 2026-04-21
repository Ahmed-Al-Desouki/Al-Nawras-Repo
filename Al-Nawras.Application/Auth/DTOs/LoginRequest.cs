using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Auth.DTOs
{
    public record LoginRequest(string Email, string Password);
}
