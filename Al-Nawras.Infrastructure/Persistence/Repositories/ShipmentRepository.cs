using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly AppDbContext _context;

        public ShipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Shipment> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Shipments
                .Include(s => s.Deal)
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public async Task<List<Shipment>> GetByDealIdAsync(Guid dealId, CancellationToken cancellationToken = default)
            => await _context.Shipments
                .Where(s => s.DealId == dealId)
                .OrderByDescending(s => s.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
            => await _context.Shipments.AddAsync(shipment, cancellationToken);

        public void Update(Shipment shipment)
            => _context.Shipments.Update(shipment);
    }
}
