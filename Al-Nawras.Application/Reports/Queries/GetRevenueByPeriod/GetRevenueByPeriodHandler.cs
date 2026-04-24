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

namespace Al_Nawras.Application.Reports.Queries.GetRevenueByPeriod
{
    public class GetRevenueByPeriodHandler
    {
        private readonly IApplicationDbContext _context;

        public GetRevenueByPeriodHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RevenueByPeriodDto>> Handle(
            GetRevenueByPeriodQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query.PeriodEnd <= query.PeriodStart)
                return Result<RevenueByPeriodDto>.Failure("PeriodEnd must be after PeriodStart.");

            // Load all payments in the window — small enough to process in memory
            var payments = await _context.Payments
                .Include(p => p.Deal)
                .Where(p => p.CreatedAt >= query.PeriodStart
                         && p.CreatedAt <= query.PeriodEnd)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load all deals in window for deal counts
            var deals = await _context.Deals
                .Where(d => d.CreatedAt >= query.PeriodStart
                         && d.CreatedAt <= query.PeriodEnd)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Group payments by chosen period
            var rows = query.Grouping switch
            {
                ReportGrouping.Monthly => BuildMonthlyRows(payments, deals, query),
                ReportGrouping.Quarterly => BuildQuarterlyRows(payments, deals, query),
                ReportGrouping.Yearly => BuildYearlyRows(payments, deals, query),
                _ => BuildMonthlyRows(payments, deals, query)
            };

            var dto = new RevenueByPeriodDto(
                TotalRevenueUSD: payments.Where(p => p.Status == PaymentStatus.FullyPaid).Sum(p => p.AmountUSD),
                TotalPendingUSD: payments.Where(p => p.Status == PaymentStatus.Pending
                                                      || p.Status == PaymentStatus.PartiallyPaid).Sum(p => p.AmountUSD),
                TotalOverdueUSD: payments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.AmountUSD),
                TotalDeals: deals.Count,
                TotalPaidPayments: payments.Count(p => p.Status == PaymentStatus.FullyPaid),
                Rows: rows
            );

            return Result<RevenueByPeriodDto>.Success(dto);
        }

        // ── Builders ───────────────────────────────────────────────────────────────

        private static List<RevenuePeriodRowDto> BuildMonthlyRows(
            List<Domain.Entities.Payment> payments,
            List<Domain.Entities.Deal> deals,
            GetRevenueByPeriodQuery query)
        {
            var rows = new List<RevenuePeriodRowDto>();
            var cursor = new DateTime(query.PeriodStart.Year, query.PeriodStart.Month, 1);
            var end = new DateTime(query.PeriodEnd.Year, query.PeriodEnd.Month, 1);

            while (cursor <= end)
            {
                var y = cursor.Year;
                var m = cursor.Month;

                var periodPayments = payments
                    .Where(p => p.CreatedAt.Year == y && p.CreatedAt.Month == m)
                    .ToList();

                var periodDeals = deals
                    .Where(d => d.CreatedAt.Year == y && d.CreatedAt.Month == m)
                    .ToList();

                rows.Add(BuildRow(
                    label: cursor.ToString("MMM yyyy"),
                    year: y,
                    month: m,
                    quarter: null,
                    payments: periodPayments,
                    deals: periodDeals
                ));

                cursor = cursor.AddMonths(1);
            }

            return rows;
        }

        private static List<RevenuePeriodRowDto> BuildQuarterlyRows(
            List<Domain.Entities.Payment> payments,
            List<Domain.Entities.Deal> deals,
            GetRevenueByPeriodQuery query)
        {
            var rows = new List<RevenuePeriodRowDto>();

            var startQ = ((query.PeriodStart.Month - 1) / 3) + 1;
            var endQ = ((query.PeriodEnd.Month - 1) / 3) + 1;

            for (var year = query.PeriodStart.Year; year <= query.PeriodEnd.Year; year++)
            {
                var fromQ = year == query.PeriodStart.Year ? startQ : 1;
                var toQ = year == query.PeriodEnd.Year ? endQ : 4;

                for (var q = fromQ; q <= toQ; q++)
                {
                    var startMonth = (q - 1) * 3 + 1;
                    var endMonth = startMonth + 2;

                    var periodPayments = payments
                        .Where(p => p.CreatedAt.Year == year
                                 && p.CreatedAt.Month >= startMonth
                                 && p.CreatedAt.Month <= endMonth)
                        .ToList();

                    var periodDeals = deals
                        .Where(d => d.CreatedAt.Year == year
                                 && d.CreatedAt.Month >= startMonth
                                 && d.CreatedAt.Month <= endMonth)
                        .ToList();

                    rows.Add(BuildRow(
                        label: $"Q{q} {year}",
                        year: year,
                        month: null,
                        quarter: q,
                        payments: periodPayments,
                        deals: periodDeals
                    ));
                }
            }

            return rows;
        }

        private static List<RevenuePeriodRowDto> BuildYearlyRows(
            List<Domain.Entities.Payment> payments,
            List<Domain.Entities.Deal> deals,
            GetRevenueByPeriodQuery query)
        {
            var rows = new List<RevenuePeriodRowDto>();

            for (var year = query.PeriodStart.Year; year <= query.PeriodEnd.Year; year++)
            {
                var periodPayments = payments
                    .Where(p => p.CreatedAt.Year == year)
                    .ToList();

                var periodDeals = deals
                    .Where(d => d.CreatedAt.Year == year)
                    .ToList();

                rows.Add(BuildRow(
                    label: year.ToString(),
                    year: year,
                    month: null,
                    quarter: null,
                    payments: periodPayments,
                    deals: periodDeals
                ));
            }

            return rows;
        }

        private static RevenuePeriodRowDto BuildRow(
            string label, int year, int? month, int? quarter,
            List<Domain.Entities.Payment> payments,
            List<Domain.Entities.Deal> deals)
        {
            var collected = payments
                .Where(p => p.Status == PaymentStatus.FullyPaid)
                .Sum(p => p.AmountUSD);

            var pending = payments
                .Where(p => p.Status == PaymentStatus.Pending
                         || p.Status == PaymentStatus.PartiallyPaid)
                .Sum(p => p.AmountUSD);

            var overdue = payments
                .Where(p => p.Status == PaymentStatus.Overdue)
                .Sum(p => p.AmountUSD);

            var dealCount = deals.Count;
            var avgDealValue = dealCount > 0
                ? deals.Average(d => (double)d.TotalValue)
                : 0;

            return new RevenuePeriodRowDto(
                PeriodLabel: label,
                Year: year,
                Month: month,
                Quarter: quarter,
                CollectedUSD: collected,
                PendingUSD: pending,
                OverdueUSD: overdue,
                DealCount: dealCount,
                PaymentCount: payments.Count,
                AvgDealValueUSD: (decimal)avgDealValue
            );
        }
    }
}
