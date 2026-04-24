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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);
            builder.ToTable("AuditLogs");
            builder.HasQueryFilter(a => true);
            builder.Property(a => a.TableName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.RecordId).IsRequired().HasMaxLength(50);
            builder.Property(a => a.Action).HasConversion<int>();
            builder.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(a => a.NewValues).HasColumnType("nvarchar(max)");
            builder.Property(a => a.IpAddress).HasMaxLength(50);

            builder.HasIndex(a => new { a.TableName, a.RecordId, a.CreatedAt });
            builder.HasIndex(a => new { a.PerformedByUserId, a.CreatedAt });

            builder.HasOne(a => a.PerformedByUser)
                   .WithMany()
                   .HasForeignKey(a => a.PerformedByUserId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);
        }
    }
}
