using Al_Nawras.Application.ClientPortal.DTOs;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyDeals
{
    public class GetMyDealsHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMyDealsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PortalDealDto>>> Handle(
            GetMyDealsQuery query,
            CancellationToken cancellationToken = default)
        {
            var dealsQuery = _context.Deals
                .Where(d => d.ClientId == query.ClientId);   // hard filter — no bypass possible

            if (query.StatusFilter.HasValue)
                dealsQuery = dealsQuery.Where(d => d.Status == query.StatusFilter.Value);

            var deals = await dealsQuery
                .Select(d => new PortalDealDto(
                    d.Id,
                    d.DealNumber,
                    d.Status.ToString(),
                    d.Commodity,
                    d.TotalValue,
                    d.Currency,
                    d.Origin,
                    d.Destination,
                    d.ConfirmedAt,
                    d.CreatedAt,
                    d.Shipments.Count,
                    d.Payments.Count(p => p.Status == PaymentStatus.FullyPaid),
                    d.Payments.Count
                ))
                .OrderByDescending(d => d.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<List<PortalDealDto>>.Success(deals);
        }
    }
}
