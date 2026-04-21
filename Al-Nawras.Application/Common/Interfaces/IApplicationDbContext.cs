using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Role> Roles { get; }
        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Client> Clients { get; }
        DbSet<Deal> Deals { get; }
        DbSet<DealStatusHistory> DealStatusHistory { get; }
        DbSet<Shipment> Shipments { get; }
        DbSet<Payment> Payments { get; }
        DbSet<Document> Documents { get; }
        DbSet<DealTask> Tasks { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<CurrencyRate> CurrencyRates { get; }
        DbSet<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
