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
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Include(c => c.AssignedSalesUser)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public async Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Clients
                .Include(c => c.AssignedSalesUser)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _context.Clients
                .AnyAsync(c => c.Email == email, cancellationToken);

        public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
            => await _context.Clients.AddAsync(client, cancellationToken);

        public void Update(Client client)
            => _context.Clients.Update(client);

        public bool Exists(Guid id)
            => _context.Clients.Any(c => c.Id == id);
    }
}
