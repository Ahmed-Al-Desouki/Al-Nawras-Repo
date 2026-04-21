using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Repositories
{
    public class DealRepository : IDealRepository
    {
        private readonly AppDbContext _context;

        public DealRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Deal> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        public async Task<Deal> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Deals
                .Include(d => d.Client)
                .Include(d => d.AssignedSalesUser)
                .Include(d => d.Shipments)
                .Include(d => d.Payments)
                .Include(d => d.Documents)
                .Include(d => d.Tasks)
                .Include(d => d.StatusHistory.OrderByDescending(h => h.ChangedAt))
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        public async Task<List<Deal>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Deals
                .Include(d => d.Client)
                .Include(d => d.AssignedSalesUser)
                .OrderByDescending(d => d.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<List<Deal>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
            => await _context.Deals
                .Include(d => d.Client)
                .Include(d => d.AssignedSalesUser)
                .Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<List<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default)
            => await _context.Deals
                .Include(d => d.Client)
                .Include(d => d.AssignedSalesUser)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<List<Deal>> GetByAssignedUserAsync(int userId, CancellationToken cancellationToken = default)
            => await _context.Deals
                .Include(d => d.Client)
                .Include(d => d.AssignedSalesUser)
                .Where(d => d.AssignedSalesUserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Deal deal, CancellationToken cancellationToken = default)
            => await _context.Deals.AddAsync(deal, cancellationToken);

        public void Update(Deal deal)
            => _context.Deals.Update(deal);

        public bool Exists(Guid id)
            => _context.Deals.Any(d => d.Id == id);
    }
}
