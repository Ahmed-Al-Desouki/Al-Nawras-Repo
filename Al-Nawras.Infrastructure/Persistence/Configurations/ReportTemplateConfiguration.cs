using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Al_Nawras.Infrastructure.Persistence.Configurations
{
    public class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
    {
        public void Configure(EntityTypeBuilder<ReportTemplate> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Slug).IsRequired().HasMaxLength(120);
            builder.Property(t => t.Description).HasMaxLength(2000);
            builder.Property(t => t.DefinitionJson).IsRequired();
            builder.Property(t => t.Category).HasConversion<int>();

            builder.HasIndex(t => t.Slug).IsUnique();
            builder.HasIndex(t => t.Category);

            builder.HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
