using Al_Nawras.Application.ClientPortal.DTOs;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyDealDetail
{
    public class GetMyDealDetailHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMyDealDetailHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PortalDealDetailDto>> Handle(
            GetMyDealDetailQuery query,
            CancellationToken cancellationToken = default)
        {
            var deal = await _context.Deals
                .Where(d => d.Id == query.DealId && d.ClientId == query.ClientId)
                .Select(d => new PortalDealDetailDto(
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
                    d.Shipments
                        .OrderByDescending(s => s.CreatedAt)
                        .Select(s => new PortalShipmentDto(
                            s.Id,
                            s.DealId,
                            s.ShipmentNumber,
                            s.Status.ToString(),
                            s.TrackingNumber,
                            s.Carrier,
                            s.VesselName,
                            s.PortOfLoading,
                            s.PortOfDischarge,
                            s.ETD,
                            s.ETA,
                            s.ActualDeparture,
                            s.ActualArrival
                        ))
                        .ToList(),
                    d.Payments
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
                        .ToList(),
                    d.Documents
                        .OrderByDescending(doc => doc.CreatedAt)
                        .Select(doc => new PortalDocumentDto(
                            doc.Id,
                            doc.DocumentType.ToString(),
                            doc.FileName,
                            doc.FileSizeBytes,
                            doc.MimeType,
                            doc.CreatedAt
                        ))
                        .ToList(),
                    d.StatusHistory
                        .OrderByDescending(h => h.ChangedAt)
                        .Select(h => new PortalStatusEventDto(
                            h.FromStatus.ToString(),
                            h.ToStatus.ToString(),
                            h.ChangedAt
                        ))
                        .ToList()
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            return deal is null
                ? Result<PortalDealDetailDto>.Failure("Deal not found.")
                : Result<PortalDealDetailDto>.Success(deal);
        }
    }
}
