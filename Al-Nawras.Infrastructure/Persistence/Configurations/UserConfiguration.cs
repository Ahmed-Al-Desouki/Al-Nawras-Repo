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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.ClientId)
                   .HasFilter("[ClientId] IS NOT NULL");

            builder.HasOne(u => u.Role)
                   .WithMany()
                   .HasForeignKey(u => u.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Client)
                   .WithMany()
                   .HasForeignKey(u => u.ClientId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);

            builder.Property(u => u.PasswordHash).IsRequired(false).HasMaxLength(500);
            builder.Property(u => u.GoogleId).HasMaxLength(100).IsRequired(false);
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(500).IsRequired(false);
            builder.HasIndex(u => u.GoogleId)
                   .HasFilter("[GoogleId] IS NOT NULL")
                   .IsUnique();


            builder.HasData(new
            {
                Id = 1,
                Email = "admin@importexport.com",
                PasswordHash = "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.",
                FirstName = "System",
                LastName = "Admin",
                RoleId = 1,
                ClientId = (Guid?)null,
                IsActive = true,
                LastLoginAt = (DateTime?)null,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
