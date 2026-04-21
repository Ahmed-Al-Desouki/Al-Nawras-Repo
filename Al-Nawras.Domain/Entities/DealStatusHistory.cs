using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class DealStatusHistory
    {
        public Guid Id { get; private set; }
        public Guid DealId { get; private set; }
        public DealStatus FromStatus { get; private set; }
        public DealStatus ToStatus { get; private set; }
        public int ChangedByUserId { get; private set; }
        public string Notes { get; private set; }
        public DateTime ChangedAt { get; private set; }

        public Deal Deal { get; private set; }
        public User ChangedByUser { get; private set; }

        private DealStatusHistory() { }

        internal DealStatusHistory(Guid dealId, DealStatus fromStatus,
            DealStatus toStatus, int changedByUserId, string notes)
        {
            Id = Guid.NewGuid();
            DealId = dealId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedByUserId = changedByUserId;
            Notes = notes;
            ChangedAt = DateTime.UtcNow;
        }
    }
}
