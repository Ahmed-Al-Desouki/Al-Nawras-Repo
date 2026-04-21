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

    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DocumentType).HasConversion<int>();
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.StoragePath).IsRequired().HasMaxLength(500);
            builder.Property(d => d.MimeType).HasMaxLength(100);

            builder.HasIndex(d => d.DealId);
            builder.HasIndex(d => d.ShipmentId);

            builder.HasOne(d => d.Shipment)
                   .WithMany(s => s.Documents)
                   .HasForeignKey(d => d.ShipmentId)
                   .OnDelete(DeleteBehavior.NoAction)
                   .IsRequired(false);

            builder.HasOne(d => d.Deal)
               .WithMany(d => d.Documents)
               .HasForeignKey(d => d.DealId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(d => d.UploadedByUser)
                   .WithMany()
                   .HasForeignKey(d => d.UploadedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
