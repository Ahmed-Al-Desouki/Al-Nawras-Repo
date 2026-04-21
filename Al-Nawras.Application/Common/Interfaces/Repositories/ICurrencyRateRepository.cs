using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces.Repositories
{
    public interface ICurrencyRateRepository
    {
        Task<CurrencyRate> GetLatestRateAsync(string fromCurrency, string toCurrency,
            CancellationToken cancellationToken = default);
        Task<CurrencyRate> GetRateByDateAsync(string fromCurrency, string toCurrency,
            DateOnly date, CancellationToken cancellationToken = default);
        Task AddAsync(CurrencyRate rate, CancellationToken cancellationToken = default);
    }
}
