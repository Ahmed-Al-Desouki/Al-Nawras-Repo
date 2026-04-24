using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Clients.Commands.CreateClient
{
    public class CreateClientHandler
    {
        private readonly IClientRepository _clientRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientHandler(
            IClientRepository clientRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateClientCommand command,
            CancellationToken cancellationToken = default)
        {
            var emailExists = await _clientRepository.ExistsByEmailAsync(command.Email, cancellationToken);
            if (emailExists)
                return Result<Guid>.Failure($"A client with email {command.Email} already exists.");

            var client = new Client(
                command.Name,
                command.Email,
                command.Phone,
                command.Country,
                command.CompanyName,
                command.AssignedSalesUserId
            );

            await _clientRepository.AddAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (command.AssignedSalesUserId.HasValue)
            {
                await _notificationDispatcher.DispatchAsync(
                    new WorkflowNotificationRequest(
                        Type: NotificationType.TaskAssigned,
                        Title: "New client created",
                        Body: $"Client {client.CompanyName} has been added and assigned to you.",
                        RelatedEntityId: client.Id,
                        RelatedEntityType: nameof(Client),
                        UserIds: [command.AssignedSalesUserId.Value],
                        ClientId: client.Id,
                        SendEmailToClient: true,
                        PushToClient: false),
                    cancellationToken);
            }

            return Result<Guid>.Success(client.Id);
        }
    }
}
