using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus
{
    public class UpdateShipmentStatusHandler
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShipmentStatusHandler(
            IShipmentRepository shipmentRepository,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateShipmentStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(command.ShipmentId, cancellationToken);

            if (shipment is null)
                return Result.Failure($"Shipment {command.ShipmentId} not found.");

            shipment.UpdateStatus(command.NewStatus, command.TrackingNumber);
            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.ShipmentDelayed,
                    Title: $"Shipment moved to {command.NewStatus}",
                    Body: $"Shipment {shipment.ShipmentNumber} is now {command.NewStatus} for deal {shipment.Deal?.DealNumber}.",
                    RelatedEntityId: shipment.Id,
                    RelatedEntityType: nameof(Al_Nawras.Domain.Entities.Shipment),
                    UserIds: shipment.Deal is null ? Array.Empty<int>() : [shipment.Deal.AssignedSalesUserId],
                    RoleNames: ["operations", "sales", "admin"],
                    ClientId: shipment.Deal?.ClientId,
                    SendEmailToClient: command.NewStatus is ShipmentStatus.InTransit or ShipmentStatus.Delivered or ShipmentStatus.AtCustoms),
                cancellationToken);

            return Result.Success();
        }
    }
}
