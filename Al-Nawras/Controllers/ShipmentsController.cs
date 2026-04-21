using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Shipments.Commands.CreateShipment;
using Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus;
using Al_Nawras.Application.Shipments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalOnly")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CreateShipmentHandler _createHandler;
        private readonly UpdateShipmentStatusHandler _updateStatusHandler;
        private readonly IShipmentRepository _shipmentRepository;

        public ShipmentsController(
            CreateShipmentHandler createHandler,
            UpdateShipmentStatusHandler updateStatusHandler,
            IShipmentRepository shipmentRepository)
        {
            _createHandler = createHandler;
            _updateStatusHandler = updateStatusHandler;
            _shipmentRepository = shipmentRepository;
        }

        [HttpGet("deal/{dealId:guid}")]
        public async Task<IActionResult> GetByDeal(Guid dealId, CancellationToken cancellationToken)
        {
            var shipments = await _shipmentRepository.GetByDealIdAsync(dealId, cancellationToken);

            var result = shipments.Select(s => new ShipmentDto(
                s.Id, s.DealId, s.Deal?.DealNumber ?? "",
                s.ShipmentNumber, s.Status, s.Status.ToString(),
                s.TrackingNumber ?? "", s.Carrier ?? "",
                s.VesselName ?? "", s.PortOfLoading ?? "",
                s.PortOfDischarge ?? "", s.ETD, s.ETA,
                s.ActualDeparture, s.ActualArrival, s.CreatedAt
            ));

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "OpsOrAdmin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateShipmentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetByDeal),
                new { dealId = command.DealId }, new { id = result.Value });
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Policy = "OpsOrAdmin")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateShipmentStatusCommand command,
            CancellationToken cancellationToken)
        {
            var cmd = command with { ShipmentId = id };
            var result = await _updateStatusHandler.Handle(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
