using Al_Nawras.Application.ClientPortal.DTOs;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyPayments
{
    public class GetMyPaymentsHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMyPaymentsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PortalPaymentDto>>> Handle(
            GetMyPaymentsQuery query,
            CancellationToken cancellationToken = default)
        {
            var paymentsQuery = _context.Payments
                .Where(p => p.Deal.ClientId == query.ClientId);

            if (query.StatusFilter.HasValue)
                paymentsQuery = paymentsQuery.Where(p => p.Status == query.StatusFilter.Value);

            var payments = await paymentsQuery
                .OrderBy(p => p.DueDate)
                .Select(p => new PortalPaymentDto(
                    p.Id,
                    p.PaymentReference,
                    p.Amount,
                    p.Currency,
                    p.Status.ToString(),
                    p.PaymentType.ToString(),
                    p.DueDate,
                    p.PaidAt
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<List<PortalPaymentDto>>.Success(payments);
        }
    }
}
