using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Deals.Commands.CreateDeal
{
    public class CreateDealHandler
    {
        private readonly IDealRepository _dealRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDealHandler(
            IDealRepository dealRepository,
            IClientRepository clientRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _dealRepository = dealRepository;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateDealCommand command,
            CancellationToken cancellationToken = default)
        {
            // Validate client exists
            if (!_clientRepository.Exists(command.ClientId))
                return Result<Guid>.Failure($"Client with ID {command.ClientId} not found.");

            // Validate assigned sales user exists
            var salesUser = await _userRepository.GetByIdAsync(command.AssignedSalesUserId, cancellationToken);
            if (salesUser is null)
                return Result<Guid>.Failure($"User with ID {command.AssignedSalesUserId} not found.");

            // Create the deal — domain object controls its own state
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

            // Notify the assigned sales user
            var notification = new Notification(
                userId: command.AssignedSalesUserId,
                type: Domain.Enums.NotificationType.DealStatusChanged,
                title: "New deal assigned to you",
                body: $"Deal {deal.DealNumber} for {command.Commodity} has been created and assigned to you.",
                relatedEntityId: deal.Id,
                relatedEntityType: "Deal"
            );

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(deal.Id);
        }
    }
}
