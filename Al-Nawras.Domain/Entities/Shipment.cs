using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Shipment
    {
        public Guid Id { get; private set; }
        public Guid DealId { get; private set; }
        public string ShipmentNumber { get; private set; }
        public ShipmentStatus Status { get; private set; }
        public string TrackingNumber { get; private set; }
        public string Carrier { get; private set; }
        public string VesselName { get; private set; }
        public string PortOfLoading { get; private set; }
        public string PortOfDischarge { get; private set; }
        public DateTime? ETD { get; private set; }
        public DateTime? ETA { get; private set; }
        public DateTime? ActualDeparture { get; private set; }
        public DateTime? ActualArrival { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Deal Deal { get; private set; }
        public ICollection<Document> Documents { get; private set; } = new List<Document>();

        private Shipment() { }

        public Shipment(Guid dealId, string carrier, string portOfLoading,
            string portOfDischarge, DateTime? etd = null, DateTime? eta = null)
        {
            Id = Guid.NewGuid();
            DealId = dealId;
            ShipmentNumber = $"SH-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            Status = ShipmentStatus.Pending;
            Carrier = carrier;
            PortOfLoading = portOfLoading;
            PortOfDischarge = portOfDischarge;
            ETD = etd;
            ETA = eta;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(ShipmentStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
            if (newStatus == ShipmentStatus.InTransit) ActualDeparture = DateTime.UtcNow;
            if (newStatus == ShipmentStatus.Delivered) ActualArrival = DateTime.UtcNow;
        }
    }
}
