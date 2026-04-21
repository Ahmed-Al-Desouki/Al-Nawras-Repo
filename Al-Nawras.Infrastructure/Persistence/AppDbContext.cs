using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<DealStatusHistory> DealStatusHistory => Set<DealStatusHistory>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DealTask> Tasks => Set<DealTask>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Picks up all IEntityTypeConfiguration<T> classes in this assembly automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
