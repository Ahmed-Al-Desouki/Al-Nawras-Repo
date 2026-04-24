using Al_Nawras.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize(Policy = "AdminOnly")]
    public class JobsController : ControllerBase
    {
        private readonly IOverduePaymentJob _overduePaymentJob;

        public JobsController(IOverduePaymentJob overduePaymentJob)
        {
            _overduePaymentJob = overduePaymentJob;
        }

        /// <summary>
        /// Manually trigger the overdue payment job.
        /// Admin only — use this during development or to force an immediate run.
        /// </summary>
        [HttpPost("overdue-payments/run")]
        public async Task<IActionResult> RunOverduePaymentJob(CancellationToken cancellationToken)
        {
            var result = await _overduePaymentJob.RunAsync(cancellationToken);

            return Ok(new
            {
                result.ProcessedCount,
                result.MarkedOverdueCount,
                result.NotificationsSentCount,
                result.RanAt,
                DurationSeconds = result.Duration.TotalSeconds,
                result.Errors,
                result.HasErrors
            });
        }
    }
}
