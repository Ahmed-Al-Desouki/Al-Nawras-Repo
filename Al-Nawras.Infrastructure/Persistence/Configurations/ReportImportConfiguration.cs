using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Al_Nawras.Infrastructure.Persistence.Configurations
{
    public class ReportImportConfiguration : IEntityTypeConfiguration<ReportImport>
    {
        public void Configure(EntityTypeBuilder<ReportImport> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
            builder.Property(i => i.Description).HasMaxLength(2000);
            builder.Property(i => i.SourceFileName).IsRequired().HasMaxLength(255);
            builder.Property(i => i.SourceStoragePath).IsRequired().HasMaxLength(500);
            builder.Property(i => i.WorkbookJson).IsRequired();
            builder.Property(i => i.AnalysisJson).IsRequired();
            builder.Property(i => i.Status).HasConversion<int>();
            builder.Property(i => i.ReviewNotes).HasMaxLength(2000);

            builder.HasIndex(i => i.ReportTemplateId);
            builder.HasIndex(i => i.CreatedAt);
            builder.HasIndex(i => i.Status);

            builder.HasOne(i => i.ReportTemplate)
                .WithMany(t => t.ReportImports)
                .HasForeignKey(i => i.ReportTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(i => i.UploadedByUser)
                .WithMany()
                .HasForeignKey(i => i.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.ReviewedByUser)
                .WithMany()
                .HasForeignKey(i => i.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
