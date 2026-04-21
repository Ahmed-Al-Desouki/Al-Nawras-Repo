using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{

    public class CurrencyRate
    {
        public int Id { get; private set; }
        public string FromCurrency { get; private set; }
        public string ToCurrency { get; private set; }
        public decimal Rate { get; private set; }
        public DateOnly RateDate { get; private set; }
        public string Source { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private CurrencyRate() { }

        public CurrencyRate(string fromCurrency, string toCurrency,
            decimal rate, DateOnly rateDate, string source)
        {
            FromCurrency = fromCurrency.ToUpper();
            ToCurrency = toCurrency.ToUpper();
            Rate = rate;
            RateDate = rateDate;
            Source = source;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
