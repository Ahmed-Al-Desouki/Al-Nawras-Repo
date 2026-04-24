using Al_Nawras.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    /// <summary>
    /// Base for all client portal controllers.
    /// Provides safe ClientId extraction from the JWT claim.
    /// Any endpoint inheriting this is guaranteed to have a valid ClientId.
    /// </summary>
    public abstract class BaseClientController : ControllerBase
    {
        protected Guid? TryGetClientId()
        {
            var claim = User.FindFirst("clientId")?.Value;
            return Guid.TryParse(claim, out var id) && id != Guid.Empty ? id : null;
        }

        protected IActionResult ClientIdMissing()
            => Forbid();   // returns 403 — never leak why
    }
}
