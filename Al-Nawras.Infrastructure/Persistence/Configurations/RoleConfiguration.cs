using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
            builder.Property(r => r.Description).HasMaxLength(200);
            builder.HasIndex(r => r.Name).IsUnique();

            // Seed data
            builder.HasData(
                new { Id = 1, Name = "Admin", Description = "Full system access" },
                new { Id = 2, Name = "Sales", Description = "Manage clients and deals" },
                new { Id = 3, Name = "Operations", Description = "Manage shipments and logistics" },
                new { Id = 4, Name = "Accounts", Description = "Manage payments and finance" },
                new { Id = 5, Name = "Client", Description = "External client portal access" }
            );
        }
    }
}
