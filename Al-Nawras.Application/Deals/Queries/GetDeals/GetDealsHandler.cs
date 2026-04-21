using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Deals.DTOs;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Queries.GetDeals
{
    public class GetDealsHandler
    {
        private readonly IDealRepository _dealRepository;

        public GetDealsHandler(IDealRepository dealRepository)
        {
            _dealRepository = dealRepository;
        }

        public async Task<Result<List<DealSummaryDto>>> Handle(
            GetDealsQuery query,
            CancellationToken cancellationToken = default)
        {
            List<Deal> deals;

            if (query.Status.HasValue)
                deals = await _dealRepository.GetByStatusAsync(query.Status.Value, cancellationToken);
            else if (query.ClientId.HasValue)
                deals = await _dealRepository.GetByClientIdAsync(query.ClientId.Value, cancellationToken);
            else if (query.AssignedUserId.HasValue)
                deals = await _dealRepository.GetByAssignedUserAsync(query.AssignedUserId.Value, cancellationToken);
            else
                deals = await _dealRepository.GetAllAsync(cancellationToken);

            var result = deals.Select(d => new DealSummaryDto(
                Id: d.Id,
                DealNumber: d.DealNumber,
                ClientName: d.Client?.Name ?? "",
                Status: d.Status,
                StatusLabel: d.Status.ToString(),
                Commodity: d.Commodity,
                TotalValue: d.TotalValue,
                Currency: d.Currency,
                Origin: d.Origin,
                Destination: d.Destination,
                CreatedAt: d.CreatedAt
            )).ToList();

            return Result<List<DealSummaryDto>>.Success(result);
        }
    }
}
