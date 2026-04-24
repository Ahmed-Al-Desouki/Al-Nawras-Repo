using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Shipments.Commands.CreateShipment
{
    public class CreateShipmentHandler
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IDealRepository _dealRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShipmentHandler(
            IShipmentRepository shipmentRepository,
            IDealRepository dealRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _dealRepository = dealRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateShipmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            if (deal.Status < DealStatus.Confirmed)
                return Result<Guid>.Failure(
                    $"Cannot create a shipment for a deal in status '{deal.Status}'. " +
                    "Deal must be at least Confirmed.");

            if (deal.Status == DealStatus.Closed)
                return Result<Guid>.Failure("Cannot create a shipment for a closed deal.");

            var shipment = new Shipment(
                command.DealId,
                command.Carrier,
                command.PortOfLoading,
                command.PortOfDischarge,
                command.ETD,
                command.ETA
            );

            await _shipmentRepository.AddAsync(shipment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DealStatusChanged,
                    Title: "Shipment created",
                    Body: $"Shipment {shipment.ShipmentNumber} has been created for deal {deal.DealNumber}.",
                    RelatedEntityId: shipment.Id,
                    RelatedEntityType: nameof(Shipment),
                    UserIds: [deal.AssignedSalesUserId],
                    RoleNames: ["operations", "sales", "admin"],
                    ClientId: deal.ClientId),
                cancellationToken);

            return Result<Guid>.Success(shipment.Id);
        }
    }
}
