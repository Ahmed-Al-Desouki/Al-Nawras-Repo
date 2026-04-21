using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetUnreadByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        void Update(Notification notification);
    }
}
