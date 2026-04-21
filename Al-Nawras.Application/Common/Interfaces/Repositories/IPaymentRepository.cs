using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Payment>> GetByDealIdAsync(Guid dealId, CancellationToken cancellationToken = default);
        Task<List<Payment>> GetOverdueAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
        void Update(Payment payment);
    }
}
