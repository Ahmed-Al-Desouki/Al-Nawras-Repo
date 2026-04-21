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
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.ShipmentNumber).IsRequired().HasMaxLength(30);
            builder.Property(s => s.Status).HasConversion<int>();
            builder.Property(s => s.TrackingNumber).HasMaxLength(100);
            builder.Property(s => s.Carrier).HasMaxLength(100);
            builder.Property(s => s.VesselName).HasMaxLength(100);
            builder.Property(s => s.PortOfLoading).HasMaxLength(100);
            builder.Property(s => s.PortOfDischarge).HasMaxLength(100);
            builder.HasOne(s => s.Deal)
               .WithMany(d => d.Shipments)
               .HasForeignKey(s => s.DealId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.DealId);
            builder.HasIndex(s => s.Status);
        }
    }
}
