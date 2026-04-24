using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reports.DTOs;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Reports.Queries.GetEmployeePerformance
{
    public class GetEmployeePerformanceHandler
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeePerformanceHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<EmployeePerformanceDto>> Handle(
            GetEmployeePerformanceQuery query,
            CancellationToken cancellationToken = default)
        {
            // Load internal employees only (not clients — RoleId != 5)
            var usersQuery = _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.RoleId != 5);

            if (query.UserId.HasValue)
                usersQuery = usersQuery.Where(u => u.Id == query.UserId.Value);

            var users = await usersQuery.AsNoTracking().ToListAsync(cancellationToken);

            // Load all deals and payments in the window — one query each
            var deals = await _context.Deals
                .Include(d => d.Payments)
                .Where(d => d.CreatedAt >= query.PeriodStart
                         && d.CreatedAt <= query.PeriodEnd)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load all unique clients per sales user
            var clientsPerUser = await _context.Deals
                .Where(d => d.CreatedAt >= query.PeriodStart
                         && d.CreatedAt <= query.PeriodEnd)
                .GroupBy(d => d.AssignedSalesUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    ClientCount = g.Select(d => d.ClientId).Distinct().Count()
                })
                .ToListAsync(cancellationToken);

            var rows = users.Select(user =>
            {
                var userDeals = deals
                    .Where(d => d.AssignedSalesUserId == user.Id)
                    .ToList();

                var closedDeals = userDeals
                    .Where(d => d.Status == DealStatus.Closed && d.ClosedAt.HasValue)
                    .ToList();

                var avgDaysToClose = closedDeals.Count > 0
                    ? closedDeals.Average(d => (d.ClosedAt!.Value - d.CreatedAt).TotalDays)
                    : 0;

                var collectedUSD = userDeals
                    .SelectMany(d => d.Payments)
                    .Where(p => p.Status == PaymentStatus.FullyPaid)
                    .Sum(p => p.AmountUSD);

                var totalDealValueUSD = userDeals.Sum(d => d.TotalValue);

                var closeRate = userDeals.Count > 0
                    ? (double)closedDeals.Count / userDeals.Count * 100
                    : 0;

                var activeClients = clientsPerUser
                    .FirstOrDefault(c => c.UserId == user.Id)?.ClientCount ?? 0;

                return new EmployeePerformanceRowDto(
                    UserId: user.Id,
                    FullName: $"{user.FirstName} {user.LastName}",
                    Email: user.Email,
                    Role: user.Role?.Name ?? "",
                    TotalDeals: userDeals.Count,
                    LeadCount: userDeals.Count(d => d.Status == DealStatus.Lead),
                    NegotiationCount: userDeals.Count(d => d.Status == DealStatus.Negotiation),
                    ConfirmedCount: userDeals.Count(d => d.Status == DealStatus.Confirmed),
                    ShippingCount: userDeals.Count(d => d.Status == DealStatus.Shipping),
                    DeliveredCount: userDeals.Count(d => d.Status == DealStatus.Delivered),
                    ClosedCount: closedDeals.Count,
                    TotalDealValueUSD: totalDealValueUSD,
                    CollectedRevenueUSD: collectedUSD,
                    DealCloseRate: Math.Round(closeRate, 1),
                    AvgDaysToClose: Math.Round(avgDaysToClose, 1),
                    ActiveClientsCount: activeClients
                );
            })
            .OrderByDescending(r => r.CollectedRevenueUSD)
            .ToList();

            return Result<EmployeePerformanceDto>.Success(
                new EmployeePerformanceDto(rows, query.PeriodStart, query.PeriodEnd));
        }
    }
}
