using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Shipments.Commands.UpdateShipmentStatus
{
    public class UpdateShipmentStatusHandler
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShipmentStatusHandler(
            IShipmentRepository shipmentRepository,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
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

            return Result.Success();
        }
    }
}
