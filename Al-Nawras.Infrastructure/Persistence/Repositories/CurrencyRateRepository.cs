using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Persistence.Repositories
{
    public class CurrencyRateRepository : ICurrencyRateRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CurrencyRate> GetLatestRateAsync(string fromCurrency, string toCurrency,
            CancellationToken cancellationToken = default)
            => await _context.CurrencyRates
                .Where(r => r.FromCurrency == fromCurrency.ToUpper()
                         && r.ToCurrency == toCurrency.ToUpper())
                .OrderByDescending(r => r.RateDate)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<CurrencyRate> GetRateByDateAsync(string fromCurrency, string toCurrency,
            DateOnly date, CancellationToken cancellationToken = default)
            => await _context.CurrencyRates
                .FirstOrDefaultAsync(r => r.FromCurrency == fromCurrency.ToUpper()
                                       && r.ToCurrency == toCurrency.ToUpper()
                                       && r.RateDate == date, cancellationToken);

        public async Task AddAsync(CurrencyRate rate, CancellationToken cancellationToken = default)
            => await _context.CurrencyRates.AddAsync(rate, cancellationToken);
    }
}
