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

namespace Al_Nawras.Application.Shipments.Commands.CreateShipment
{
    public class CreateShipmentHandler
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IDealRepository _dealRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShipmentHandler(
            IShipmentRepository shipmentRepository,
            IDealRepository dealRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _dealRepository = dealRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateShipmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var deal = await _dealRepository.GetByIdAsync(command.DealId, cancellationToken);

            if (deal is null)
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            // Shipments can only be created for confirmed or active deals
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

            // Notify the assigned sales user
            var notification = new Notification(
                userId: deal.AssignedSalesUserId,
                type: NotificationType.DealStatusChanged,
                title: "Shipment created",
                body: $"Shipment {shipment.ShipmentNumber} has been created for deal {deal.DealNumber}.",
                relatedEntityId: deal.Id,
                relatedEntityType: "Deal"
            );

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(shipment.Id);
        }
    }
}
