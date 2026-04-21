using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDealRepository _dealRepository;
        private readonly ICurrencyRateRepository _currencyRateRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaymentHandler(
            IPaymentRepository paymentRepository,
            IDealRepository dealRepository,
            ICurrencyRateRepository currencyRateRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _dealRepository = dealRepository;
            _currencyRateRepository = currencyRateRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreatePaymentCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            if (deal.Status == Domain.Enums.DealStatus.Closed)
                return Result<Guid>.Failure("Cannot add a payment to a closed deal.");

            // Resolve exchange rate — snapshot it at time of payment creation
            decimal exchangeRate = 1m;

            if (!command.Currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                var rate = await _currencyRateRepository
                    .GetLatestRateAsync(command.Currency, "USD", cancellationToken);

                if (rate is null)
                    return Result<Guid>.Failure(
                        $"No exchange rate found for {command.Currency} → USD. " +
                        "Please add the rate in CurrencyRates before creating this payment.");

                exchangeRate = rate.Rate;
            }

            var payment = new Payment(
                command.DealId,
                command.Amount,
                command.Currency,
                exchangeRate,
                command.PaymentType,
                command.DueDate,
                command.Notes
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Notify accounts team — role id 4
            var notification = new Notification(
                userId: deal.AssignedSalesUserId,
                type: Domain.Enums.NotificationType.DealStatusChanged,
                title: "New payment scheduled",
                body: $"Payment {payment.PaymentReference} of {command.Amount} {command.Currency} " +
                                   $"due {command.DueDate:dd MMM yyyy} added to deal {deal.DealNumber}.",
                relatedEntityId: deal.Id,
                relatedEntityType: "Deal"
            );

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(payment.Id);
        }
    }
}
