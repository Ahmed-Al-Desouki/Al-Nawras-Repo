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
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Phone).HasMaxLength(50);
            builder.Property(c => c.Country).HasMaxLength(100);
            builder.Property(c => c.CompanyName).HasMaxLength(200);

            builder.HasIndex(c => c.AssignedSalesUserId);

            builder.HasOne(c => c.AssignedSalesUser)
                   .WithMany()
                   .HasForeignKey(c => c.AssignedSalesUserId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);
        }
    }
}
