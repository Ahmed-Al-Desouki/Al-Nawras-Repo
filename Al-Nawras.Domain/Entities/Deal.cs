using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Deal  // — Aggregate Root
    { 
        private readonly List<DealStatusHistory> _statusHistory = new();
        private readonly List<Shipment> _shipments = new();
        private readonly List<Payment> _payments = new();
        private readonly List<Document> _documents = new();
        private readonly List<DealTask> _tasks = new();
        private readonly List<Notification> _notifications = new();

        public Guid Id { get; private set; }
        public string DealNumber { get; private set; }
        public Guid ClientId { get; private set; }
        public DealStatus Status { get; private set; }
        public string Commodity { get; private set; }
        public decimal TotalValue { get; private set; }
        public string Currency { get; private set; }
        public int AssignedSalesUserId { get; private set; }
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public string Notes { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Client Client { get; private set; }
        public User AssignedSalesUser { get; private set; }
        public IReadOnlyCollection<DealStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
        public IReadOnlyCollection<Shipment> Shipments => _shipments.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();
        public IReadOnlyCollection<DealTask> Tasks => _tasks.AsReadOnly();
        public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

        // Valid transition map — business rule lives in the domain
        private static readonly Dictionary<DealStatus, DealStatus[]> AllowedTransitions = new()
        {
            { DealStatus.Lead,        new[] { DealStatus.Negotiation } },
            { DealStatus.Negotiation, new[] { DealStatus.Confirmed, DealStatus.Lead } },
            { DealStatus.Confirmed,   new[] { DealStatus.Shipping } },
            { DealStatus.Shipping,    new[] { DealStatus.Customs } },
            { DealStatus.Customs,     new[] { DealStatus.Delivered } },
            { DealStatus.Delivered,   new[] { DealStatus.Closed } },
            { DealStatus.Closed,      Array.Empty<DealStatus>() }
        };

        private Deal() { }  // required by EF Core

        public Deal(Guid clientId, string commodity, decimal totalValue, string currency,
            int assignedSalesUserId, string origin, string destination, string notes = null)
        {
            Id = Guid.NewGuid();
            DealNumber = GenerateDealNumber();
            ClientId = clientId;
            Status = DealStatus.Lead;
            Commodity = commodity;
            TotalValue = totalValue;
            Currency = currency;
            AssignedSalesUserId = assignedSalesUserId;
            Origin = origin;
            Destination = destination;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public DealStatusHistory MoveToStatus(DealStatus newStatus, int changedByUserId, string notes = null)
        {
            if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
                throw new InvalidOperationException(
                    $"Transition from {Status} to {newStatus} is not allowed.");

            var historyEntry = new DealStatusHistory(Id, Status, newStatus, changedByUserId, notes);
            _statusHistory.Add(historyEntry);

            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;

            if (newStatus == DealStatus.Confirmed) ConfirmedAt = DateTime.UtcNow;
            if (newStatus == DealStatus.Closed) ClosedAt = DateTime.UtcNow;

            return historyEntry;
        }

        private static string GenerateDealNumber()
            => $"DL-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}
