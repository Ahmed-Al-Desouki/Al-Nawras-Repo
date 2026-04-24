using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Deals.Commands.MoveDealStatus
{
    public class MoveDealStatusHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IDealRepository _dealRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public MoveDealStatusHandler(
            IApplicationDbContext dbContext,
            IDealRepository dealRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _dealRepository = dealRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MoveDealStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result.Failure($"Deal {command.DealId} not found.");

            DealStatusHistory historyEntry;
            try
            {
                historyEntry = deal.MoveToStatus(command.NewStatus, command.ChangedByUserId, command.Notes);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message);
            }

            await _dbContext.DealStatusHistory.AddAsync(historyEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DealStatusChanged,
                    Title: $"Deal moved to {command.NewStatus}",
                    Body: $"Deal {deal.DealNumber} has been moved to {command.NewStatus}.",
                    RelatedEntityId: deal.Id,
                    RelatedEntityType: nameof(Deal),
                    UserIds: [deal.AssignedSalesUserId],
                    RoleNames: ["sales", "operations", "accounts", "admin"],
                    ClientId: deal.ClientId),
                cancellationToken);

            return Result.Success();
        }
    }
}
