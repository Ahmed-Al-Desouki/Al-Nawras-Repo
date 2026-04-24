using Al_Nawras.Application.AuditLogs.Queries.GetAuditHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Policy = "AdminOnly")]
    public class AuditController : ControllerBase
    {
        private readonly GetAuditHistoryHandler _handler;

        public AuditController(GetAuditHistoryHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Get the full change history for any record.
        /// Example: GET /api/audit/Deals/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </summary>
        [HttpGet("{tableName}/{recordId}")]
        public async Task<IActionResult> GetHistory(
            string tableName,
            string recordId,
            CancellationToken cancellationToken)
        {
            var result = await _handler.Handle(
                new GetAuditHistoryQuery(tableName, recordId),
                cancellationToken);

            return Ok(result.Value);
        }
    }
}
