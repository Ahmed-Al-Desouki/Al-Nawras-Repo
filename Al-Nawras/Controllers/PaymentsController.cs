using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Payments.Commands.CreatePayment;
using Al_Nawras.Application.Payments.Commands.MarkPaymentPaid;
using Al_Nawras.Application.Payments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalOnly")]
    public class PaymentsController : ControllerBase
    {
        private readonly CreatePaymentHandler _createHandler;
        private readonly MarkPaymentPaidHandler _markPaidHandler;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentsController(
            CreatePaymentHandler createHandler,
            MarkPaymentPaidHandler markPaidHandler,
            IPaymentRepository paymentRepository)
        {
            _createHandler = createHandler;
            _markPaidHandler = markPaidHandler;
            _paymentRepository = paymentRepository;
        }

        [HttpGet("deal/{dealId:guid}")]
        public async Task<IActionResult> GetByDeal(Guid dealId, CancellationToken cancellationToken)
        {
            var payments = await _paymentRepository.GetByDealIdAsync(dealId, cancellationToken);

            var result = payments.Select(p => new PaymentDto(
                p.Id, p.DealId, p.Deal?.DealNumber ?? "",
                p.PaymentReference, p.Amount, p.Currency,
                p.ExchangeRateToUSD, p.AmountUSD,
                p.Status, p.Status.ToString(),
                p.PaymentType, p.PaymentType.ToString(),
                p.DueDate, p.PaidAt, p.Notes ?? "",
                p.CreatedAt
            ));

            return Ok(result);
        }

        [HttpGet("overdue")]
        [Authorize(Policy = "AccountsOrAdmin")]
        public async Task<IActionResult> GetOverdue(CancellationToken cancellationToken)
        {
            var payments = await _paymentRepository.GetOverdueAsync(cancellationToken);

            var result = payments.Select(p => new PaymentDto(
                p.Id, p.DealId, p.Deal?.DealNumber ?? "",
                p.PaymentReference, p.Amount, p.Currency,
                p.ExchangeRateToUSD, p.AmountUSD,
                p.Status, p.Status.ToString(),
                p.PaymentType, p.PaymentType.ToString(),
                p.DueDate, p.PaidAt, p.Notes ?? "",
                p.CreatedAt
            ));

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AccountsOrAdmin")]
        public async Task<IActionResult> Create(
            [FromBody] CreatePaymentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetByDeal),
                new { dealId = command.DealId }, new { id = result.Value });
        }

        [HttpPut("{id:guid}/mark-paid")]
        [Authorize(Policy = "AccountsOrAdmin")]
        public async Task<IActionResult> MarkPaid(Guid id, CancellationToken cancellationToken)
        {
            var result = await _markPaidHandler.Handle(
                new MarkPaymentPaidCommand(id), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
