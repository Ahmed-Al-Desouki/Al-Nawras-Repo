using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Deals.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Queries.GetDealById
{
    public class GetDealByIdHandler
    {
        private readonly IDealRepository _dealRepository;

        public GetDealByIdHandler(IDealRepository dealRepository)
        {
            _dealRepository = dealRepository;
        }

        public async Task<Result<DealDto>> Handle(
            GetDealByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdWithDetailsAsync(query.DealId, cancellationToken);

            if (deal is null)
                return Result<DealDto>.Failure($"Deal {query.DealId} not found.");

            var dto = new DealDto(
                Id: deal.Id,
                DealNumber: deal.DealNumber,
                ClientId: deal.ClientId,
                ClientName: deal.Client?.Name ?? "",
                Status: deal.Status,
                StatusLabel: deal.Status.ToString(),
                Commodity: deal.Commodity,
                TotalValue: deal.TotalValue,
                Currency: deal.Currency,
                AssignedSalesUserId: deal.AssignedSalesUserId,
                AssignedSalesUserName: deal.AssignedSalesUser is null ? ""
                                       : $"{deal.AssignedSalesUser.FirstName} {deal.AssignedSalesUser.LastName}",
                Origin: deal.Origin,
                Destination: deal.Destination,
                Notes: deal.Notes,
                ConfirmedAt: deal.ConfirmedAt,
                ClosedAt: deal.ClosedAt,
                CreatedAt: deal.CreatedAt
            );

            return Result<DealDto>.Success(dto);
        }
    }
}
