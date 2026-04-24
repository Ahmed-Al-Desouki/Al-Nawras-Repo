using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Deals.Commands.CreateDeal
{
    public class CreateDealHandler
    {
        private readonly IDealRepository _dealRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDealHandler(
            IDealRepository dealRepository,
            IClientRepository clientRepository,
            IUserRepository userRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _dealRepository = dealRepository;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateDealCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!_clientRepository.Exists(command.ClientId))
                return Result<Guid>.Failure($"Client with ID {command.ClientId} not found.");

            var salesUser = await _userRepository.GetByIdAsync(command.AssignedSalesUserId, cancellationToken);
            if (salesUser is null)
                return Result<Guid>.Failure($"User with ID {command.AssignedSalesUserId} not found.");

            var deal = new Deal(
                command.ClientId,
                command.Commodity,
                command.TotalValue,
                command.Currency,
                command.AssignedSalesUserId,
                command.Origin,
                command.Destination,
                command.Notes
            );

            await _dealRepository.AddAsync(deal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DealStatusChanged,
                    Title: "New deal created",
                    Body: $"Deal {deal.DealNumber} for {command.Commodity} has been created and assigned.",
                    RelatedEntityId: deal.Id,
                    RelatedEntityType: nameof(Deal),
                    UserIds: [command.AssignedSalesUserId],
                    RoleNames: ["admin"],
                    ClientId: command.ClientId),
                cancellationToken);

            return Result<Guid>.Success(deal.Id);
        }
    }
}
