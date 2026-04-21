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
    public class DealStatusHistoryConfiguration : IEntityTypeConfiguration<DealStatusHistory>
    {
        public void Configure(EntityTypeBuilder<DealStatusHistory> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.FromStatus).HasConversion<int>();
            builder.Property(h => h.ToStatus).HasConversion<int>();
            builder.Property(h => h.Notes).HasMaxLength(500);

            builder.HasIndex(h => new { h.DealId, h.ChangedAt });

            builder.HasOne(h => h.ChangedByUser)
                   .WithMany()
                   .HasForeignKey(h => h.ChangedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
