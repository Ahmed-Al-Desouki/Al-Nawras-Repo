using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Commands.MoveDealStatus
{
    public class MoveDealStatusHandler
    {
        private readonly IDealRepository _dealRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MoveDealStatusHandler(
            IDealRepository dealRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _dealRepository = dealRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MoveDealStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result.Failure($"Deal {command.DealId} not found.");

            try
            {
                // Domain enforces the transition rules — throws if invalid
                deal.MoveToStatus(command.NewStatus, command.ChangedByUserId, command.Notes);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message);
            }

            _dealRepository.Update(deal);

            // Notify assigned user of the status change
            var notification = new Notification(
                userId: deal.AssignedSalesUserId,
                type: NotificationType.DealStatusChanged,
                title: $"Deal moved to {command.NewStatus}",
                body: $"Deal {deal.DealNumber} has been moved to {command.NewStatus}.",
                relatedEntityId: deal.Id,
                relatedEntityType: "Deal"
            );

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
