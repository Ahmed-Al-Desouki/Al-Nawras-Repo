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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PaymentReference).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            builder.Property(p => p.ExchangeRateToUSD).HasPrecision(18, 6);
            builder.Property(p => p.AmountUSD).HasPrecision(18, 2);
            builder.Property(p => p.Status).HasConversion<int>();
            builder.Property(p => p.PaymentType).HasConversion<int>();
            builder.Property(p => p.Notes).HasMaxLength(500);

            builder.HasIndex(p => p.DealId);
            builder.HasIndex(p => p.DueDate)
                   .HasFilter("[Status] != 2");  // index only unpaid payments
        }
    }
}
