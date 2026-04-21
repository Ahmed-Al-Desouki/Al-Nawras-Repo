using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Payments.Commands.MarkPaymentPaid
{
    public class MarkPaymentPaidHandler
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkPaymentPaidHandler(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MarkPaymentPaidCommand command,
            CancellationToken cancellationToken = default)
        {
            var payment = await _paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);

            if (payment is null)
                return Result.Failure($"Payment {command.PaymentId} not found.");

            if (payment.Status == Domain.Enums.PaymentStatus.FullyPaid)
                return Result.Failure("Payment is already marked as paid.");

            payment.MarkAsPaid();
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
