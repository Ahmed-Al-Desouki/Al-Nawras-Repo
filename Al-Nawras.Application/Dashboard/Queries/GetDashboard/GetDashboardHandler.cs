using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Dashboard.DTOs;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Dashboard.Queries.GetDashboard
{
    public class GetDashboardHandler
    {
        private readonly IApplicationDbContext _context;

        public GetDashboardHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DashboardDto>> Handle(
            GetDashboardQuery query,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // ── Deal counts ────────────────────────────────────────────────────────
            var dealCounts = await _context.Deals
                .Where(d => d.Status != DealStatus.Closed)
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Count(DealStatus s) => dealCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

            var totalActive = dealCounts.Sum(x => x.Count);
            var leadCount = Count(DealStatus.Lead);
            var negCount = Count(DealStatus.Negotiation);
            var confirmedCount = Count(DealStatus.Confirmed);
            var shippingCount = Count(DealStatus.Shipping);
            var customsCount = Count(DealStatus.Customs);

            // ── Shipment stats ─────────────────────────────────────────────────────
            var inTransit = await _context.Shipments
                .CountAsync(s => s.Status == ShipmentStatus.InTransit
                              || s.Status == ShipmentStatus.AtCustoms, cancellationToken);

            var delayedShipments = await _context.Shipments
                .Include(s => s.Deal)
                .Where(s => s.ETA < now
                         && s.Status != ShipmentStatus.Delivered)
                .OrderBy(s => s.ETA)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ── Payment stats ──────────────────────────────────────────────────────
            var totalRevenueUSD = await _context.Payments
                .Where(p => p.Status == PaymentStatus.FullyPaid)
                .SumAsync(p => p.AmountUSD, cancellationToken);

            var overduePayments = await _context.Payments
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Client)
                .Where(p => p.Status != PaymentStatus.FullyPaid
                         && p.DueDate < now)
                .OrderBy(p => p.DueDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var overdueUSD = overduePayments.Sum(p => p.AmountUSD);

            // ── Recent deals ───────────────────────────────────────────────────────
            var recentDeals = await _context.Deals
                .Include(d => d.Client)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // ── Map to DTOs ────────────────────────────────────────────────────────
            var dashboard = new DashboardDto(
                TotalActiveDeals: totalActive,
                LeadCount: leadCount,
                NegotiationCount: negCount,
                ConfirmedCount: confirmedCount,
                ShippingCount: shippingCount,
                CustomsCount: customsCount,
                TotalShipmentsInTransit: inTransit,
                DelayedShipmentsCount: delayedShipments.Count,
                TotalRevenueUSD: totalRevenueUSD,
                OverduePaymentsUSD: overdueUSD,
                OverduePaymentsCount: overduePayments.Count,

                RecentDeals: recentDeals.Select(d => new RecentDealDto(
                    Id: d.Id,
                    DealNumber: d.DealNumber,
                    ClientName: d.Client?.Name ?? "",
                    Status: d.Status.ToString(),
                    TotalValue: d.TotalValue,
                    Currency: d.Currency,
                    CreatedAt: d.CreatedAt
                )).ToList(),

                OverduePayments: overduePayments.Select(p => new OverduePaymentDto(
                    Id: p.Id,
                    PaymentReference: p.PaymentReference,
                    DealNumber: p.Deal?.DealNumber ?? "",
                    ClientName: p.Deal?.Client?.Name ?? "",
                    Amount: p.Amount,
                    Currency: p.Currency,
                    AmountUSD: p.AmountUSD,
                    DueDate: p.DueDate,
                    DaysOverdue: (int)(now - p.DueDate).TotalDays
                )).ToList(),

                DelayedShipments: delayedShipments.Select(s => new DelayedShipmentDto(
                    Id: s.Id,
                    ShipmentNumber: s.ShipmentNumber,
                    DealNumber: s.Deal?.DealNumber ?? "",
                    Carrier: s.Carrier ?? "",
                    PortOfDischarge: s.PortOfDischarge ?? "",
                    ETA: s.ETA,
                    DaysDelayed: s.ETA.HasValue ? (int)(now - s.ETA.Value).TotalDays : 0
                )).ToList()
            );

            return Result<DashboardDto>.Success(dashboard);
        }
    }
}
