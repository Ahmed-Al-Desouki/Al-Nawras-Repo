using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Payments.Commands.MarkPaymentPaid
{
    public class MarkPaymentPaidHandler
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public MarkPaymentPaidHandler(
            IPaymentRepository paymentRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MarkPaymentPaidCommand command,
            CancellationToken cancellationToken = default)
        {
            var payment = await _paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);

            if (payment is null)
                return Result.Failure($"Payment {command.PaymentId} not found.");

            if (payment.Status == PaymentStatus.FullyPaid)
                return Result.Failure("Payment is already marked as paid.");

            payment.MarkAsPaid();
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DealStatusChanged,
                    Title: "Payment marked as paid",
                    Body: $"Payment {payment.PaymentReference} has been marked as fully paid.",
                    RelatedEntityId: payment.Id,
                    RelatedEntityType: nameof(Payment),
                    UserIds: payment.Deal is null ? Array.Empty<int>() : [payment.Deal.AssignedSalesUserId],
                    RoleNames: ["accounts", "sales", "admin"],
                    ClientId: payment.Deal?.ClientId),
                cancellationToken);

            return Result.Success();
        }
    }
}
