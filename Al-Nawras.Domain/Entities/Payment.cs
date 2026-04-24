using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid DealId { get; private set; }
        public string PaymentReference { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public decimal ExchangeRateToUSD { get; private set; }
        public decimal AmountUSD { get; private set; }
        public PaymentStatus Status { get; private set; }
        public PaymentType PaymentType { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public string Notes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Deal Deal { get; private set; }

        private Payment() { }

        public Payment(Guid dealId, decimal amount, string currency,
            decimal exchangeRateToUSD, PaymentType paymentType, DateTime dueDate, string notes = null)
        {
            Id = Guid.NewGuid();
            DealId = dealId;
            PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            Amount = amount;
            Currency = currency;
            ExchangeRateToUSD = exchangeRateToUSD;
            AmountUSD = Math.Round(amount * exchangeRateToUSD, 2);
            PaymentType = paymentType;
            Status = PaymentStatus.Pending;
            DueDate = dueDate;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid()
        {
            Status = PaymentStatus.FullyPaid;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsOverdue()
        {
            if (Status != PaymentStatus.FullyPaid)
            {
                Status = PaymentStatus.Overdue;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>Internal — used by seeder to create payments with a specific status.</summary>
        internal void ForceStatus(PaymentStatus status, DateTime? paidAt)
        {
            // MarkAsPaid / MarkAsOverdue already exist — this just allows direct override
            if (status == PaymentStatus.FullyPaid && paidAt.HasValue)
            {
                MarkAsPaid();
            }
            else if (status == PaymentStatus.Overdue)
            {
                MarkAsOverdue();
            }
        }
    }
}
