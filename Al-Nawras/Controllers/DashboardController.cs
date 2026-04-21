using Al_Nawras.Application.Dashboard.Queries.GetDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalOnly")]
    public class DashboardController : ControllerBase
    {
        private readonly GetDashboardHandler _dashboardHandler;

        public DashboardController(GetDashboardHandler dashboardHandler)
        {
            _dashboardHandler = dashboardHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _dashboardHandler.Handle(new GetDashboardQuery(), cancellationToken);
            return Ok(result.Value);
        }
    }
}
