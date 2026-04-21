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
    public class DealConfiguration : IEntityTypeConfiguration<Deal>
    {
        public void Configure(EntityTypeBuilder<Deal> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DealNumber).IsRequired().HasMaxLength(30);
            builder.Property(d => d.Commodity).IsRequired().HasMaxLength(200);
            builder.Property(d => d.TotalValue).HasPrecision(18, 2);
            builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);
            builder.Property(d => d.Origin).HasMaxLength(200);
            builder.Property(d => d.Destination).HasMaxLength(200);
            builder.Property(d => d.Notes).HasMaxLength(1000);
            builder.Property(d => d.Status).HasConversion<int>();

            builder.HasIndex(d => d.Status);
            builder.HasIndex(d => d.ClientId);
            builder.HasIndex(d => d.CreatedAt);
            builder.HasIndex(d => d.AssignedSalesUserId);

            builder.HasOne(d => d.Client)
                   .WithMany(c => c.Deals)
                   .HasForeignKey(d => d.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.AssignedSalesUser)
                   .WithMany()
                   .HasForeignKey(d => d.AssignedSalesUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Collections backed by private fields
            builder.HasMany(d => d.StatusHistory)
                   .WithOne(h => h.Deal)
                   .HasForeignKey(h => h.DealId);

            builder.HasMany(d => d.Shipments)
                   .WithOne(s => s.Deal)
                   .HasForeignKey(s => s.DealId);

            builder.HasMany(d => d.Payments)
                   .WithOne(p => p.Deal)
                   .HasForeignKey(p => p.DealId);

            builder.HasMany(d => d.Documents)
                   .WithOne(doc => doc.Deal)
                   .HasForeignKey(doc => doc.DealId);

            builder.HasMany(d => d.Tasks)
                   .WithOne(t => t.Deal)
                   .HasForeignKey(t => t.DealId);

            builder.HasMany(d => d.Notifications)
                   .WithOne()
                   .HasForeignKey(n => n.RelatedEntityId)
                   .IsRequired(false);

            // Map private backing fields
            builder.Navigation(d => d.StatusHistory).HasField("_statusHistory");
            builder.Navigation(d => d.Shipments).HasField("_shipments");
            builder.Navigation(d => d.Payments).HasField("_payments");
            builder.Navigation(d => d.Documents).HasField("_documents");
            builder.Navigation(d => d.Tasks).HasField("_tasks");
        }
    }
}
