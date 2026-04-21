using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface IDealRepository
    {
        Task<Deal> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Deal> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Deal>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Deal>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<List<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
        Task<List<Deal>> GetByAssignedUserAsync(int userId, CancellationToken cancellationToken = default);
        Task AddAsync(Deal deal, CancellationToken cancellationToken = default);
        void Update(Deal deal);
        bool Exists(Guid id);
    }
}
