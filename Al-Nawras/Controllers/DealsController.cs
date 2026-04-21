using Al_Nawras.Application.Deals.Commands.CreateDeal;
using Al_Nawras.Application.Deals.Commands.MoveDealStatus;
using Al_Nawras.Application.Deals.Queries.GetDealById;
using Al_Nawras.Application.Deals.Queries.GetDeals;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DealsController : ControllerBase
    {
        private readonly CreateDealHandler _createHandler;
        private readonly MoveDealStatusHandler _moveStatusHandler;
        private readonly GetDealByIdHandler _getByIdHandler;
        private readonly GetDealsHandler _getDealsHandler;

        public DealsController(
            CreateDealHandler createHandler,
            MoveDealStatusHandler moveStatusHandler,
            GetDealByIdHandler getByIdHandler,
            GetDealsHandler getDealsHandler)
        {
            _createHandler = createHandler;
            _moveStatusHandler = moveStatusHandler;
            _getByIdHandler = getByIdHandler;
            _getDealsHandler = getDealsHandler;
        }

        [HttpGet]
        [Authorize(Policy = "InternalOnly")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DealStatus? status,
            [FromQuery] Guid? clientId,
            [FromQuery] int? assignedUserId,
            CancellationToken cancellationToken)
        {
            var query = new GetDealsQuery(status, clientId, assignedUserId);
            var result = await _getDealsHandler.Handle(query, cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _getByIdHandler.Handle(new GetDealByIdQuery(id), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            // Clients can only see their own deals
            if (User.IsInRole("Client"))
            {
                var clientIdClaim = User.FindFirstValue("clientId");
                if (string.IsNullOrEmpty(clientIdClaim) ||
                    result.Value.ClientId.ToString() != clientIdClaim)
                    return Forbid();
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize(Policy = "SalesOrAdmin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDealCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:guid}/move-status")]
        [Authorize(Policy = "InternalOnly")]
        public async Task<IActionResult> MoveStatus(
            Guid id,
            [FromBody] MoveStatusRequest request,
            CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new MoveDealStatusCommand(id, request.NewStatus, userId, request.Notes);
            var result = await _moveStatusHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }

    // Small inline request model — too small to need its own file
    public record MoveStatusRequest(DealStatus NewStatus, string Notes);
}
