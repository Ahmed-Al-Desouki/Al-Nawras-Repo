using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; private set; }
        public string TableName { get; private set; }
        public string RecordId { get; private set; }
        public AuditAction Action { get; private set; }
        public int? PerformedByUserId { get; private set; }
        public string OldValues { get; private set; }  // JSON
        public string NewValues { get; private set; }  // JSON
        public string IpAddress { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User PerformedByUser { get; private set; }

        private AuditLog() { }

        public AuditLog(string tableName, string recordId, AuditAction action,
            int? performedByUserId, string oldValues, string newValues, string ipAddress)
        {
            TableName = tableName;
            RecordId = recordId;
            Action = action;
            PerformedByUserId = performedByUserId;
            OldValues = oldValues;
            NewValues = newValues;
            IpAddress = ipAddress;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
