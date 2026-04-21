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
    public class DealTaskConfiguration : IEntityTypeConfiguration<DealTask>
    {
        public void Configure(EntityTypeBuilder<DealTask> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(1000);
            builder.Property(t => t.Status).HasConversion<int>();
            builder.Property(t => t.Priority).HasConversion<int>();

            builder.HasIndex(t => t.AssignedToUserId);
            builder.HasIndex(t => t.DealId);

            builder.HasOne(t => t.AssignedToUser)
                   .WithMany()
                   .HasForeignKey(t => t.AssignedToUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
