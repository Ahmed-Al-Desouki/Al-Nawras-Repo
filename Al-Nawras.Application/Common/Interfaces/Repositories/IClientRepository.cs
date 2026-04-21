using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface IClientRepository
    {
        Task<Client> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(Client client, CancellationToken cancellationToken = default);
        void Update(Client client);
        bool Exists(Guid id);
    }
}
