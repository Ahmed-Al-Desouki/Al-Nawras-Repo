using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface IShipmentRepository
    {
        Task<Shipment> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Shipment>> GetByDealIdAsync(Guid dealId, CancellationToken cancellationToken = default);
        Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
        void Update(Shipment shipment);
    }
}
