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
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(int userId, CancellationToken cancellationToken = default)
            => await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
            => await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
            => await _context.Notifications.AddAsync(notification, cancellationToken);

        public void Update(Notification notification)
            => _context.Notifications.Update(notification);
    }
}
