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
    public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
    {
        public void Configure(EntityTypeBuilder<CurrencyRate> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.FromCurrency).IsRequired().HasMaxLength(3);
            builder.Property(c => c.ToCurrency).IsRequired().HasMaxLength(3);
            builder.Property(c => c.Rate).HasPrecision(18, 6);
            builder.Property(c => c.Source).HasMaxLength(50);

            builder.HasIndex(c => new { c.FromCurrency, c.ToCurrency, c.RateDate });
        }
    }
}
