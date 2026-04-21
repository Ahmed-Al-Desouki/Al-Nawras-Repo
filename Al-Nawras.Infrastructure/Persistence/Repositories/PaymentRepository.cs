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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Payments
                .Include(p => p.Deal)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        public async Task<List<Payment>> GetByDealIdAsync(Guid dealId, CancellationToken cancellationToken = default)
            => await _context.Payments
                .Where(p => p.DealId == dealId)
                .OrderBy(p => p.DueDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<List<Payment>> GetOverdueAsync(CancellationToken cancellationToken = default)
            => await _context.Payments
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Client)
                .Where(p => p.Status != PaymentStatus.FullyPaid
                         && p.DueDate < DateTime.UtcNow)
                .OrderBy(p => p.DueDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
            => await _context.Payments.AddAsync(payment, cancellationToken);

        public void Update(Payment payment)
            => _context.Payments.Update(payment);
    }
}
