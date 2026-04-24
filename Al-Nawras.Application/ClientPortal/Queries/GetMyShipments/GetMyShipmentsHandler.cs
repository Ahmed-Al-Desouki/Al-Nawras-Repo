using Al_Nawras.Application.ClientPortal.DTOs;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyShipments
{
    public class GetMyShipmentsHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMyShipmentsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PortalShipmentDto>>> Handle(
            GetMyShipmentsQuery query,
            CancellationToken cancellationToken = default)
        {
            // Join through Deal to enforce client ownership
            var shipments = await _context.Shipments
                .Where(s => s.Deal.ClientId == query.ClientId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new PortalShipmentDto(
                    s.Id, s.DealId, s.ShipmentNumber,
                    s.Status.ToString(),
                    s.TrackingNumber, s.Carrier, s.VesselName,
                    s.PortOfLoading, s.PortOfDischarge,
                    s.ETD, s.ETA, s.ActualDeparture, s.ActualArrival
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<List<PortalShipmentDto>>.Success(shipments);
        }
    }
}
