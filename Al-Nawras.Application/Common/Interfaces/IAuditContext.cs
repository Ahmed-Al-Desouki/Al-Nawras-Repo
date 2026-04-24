using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces
{
    /// <summary>
    /// Provides the current user identity to the audit interceptor.
    /// Implemented in Infrastructure using IHttpContextAccessor.
    /// Returns null for background jobs (no HTTP context).
    /// </summary>
    public interface IAuditContext
    {
        int? CurrentUserId { get; }
        string? CurrentIpAddress { get; }
    }
}
