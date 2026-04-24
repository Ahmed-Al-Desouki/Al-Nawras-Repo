using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDealRepository _dealRepository;
        private readonly ICurrencyRateRepository _currencyRateRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaymentHandler(
            IPaymentRepository paymentRepository,
            IDealRepository dealRepository,
            ICurrencyRateRepository currencyRateRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _dealRepository = dealRepository;
            _currencyRateRepository = currencyRateRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreatePaymentCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            if (deal.Status == DealStatus.Closed)
                return Result<Guid>.Failure("Cannot add a payment to a closed deal.");

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
                command.Notes ?? string.Empty
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DealStatusChanged,
                    Title: "New payment scheduled",
                    Body: $"Payment {payment.PaymentReference} of {command.Amount} {command.Currency} due {command.DueDate:dd MMM yyyy} was added to deal {deal.DealNumber}.",
                    RelatedEntityId: payment.Id,
                    RelatedEntityType: nameof(Payment),
                    UserIds: [deal.AssignedSalesUserId],
                    RoleNames: ["accounts", "sales", "admin"],
                    ClientId: deal.ClientId),
                cancellationToken);

            return Result<Guid>.Success(payment.Id);
        }
    }
}
