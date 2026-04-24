using Al_Nawras.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Services
{
    public class AuditContext : IAuditContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? CurrentUserId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?
                    .User.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        public string? CurrentIpAddress =>
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
