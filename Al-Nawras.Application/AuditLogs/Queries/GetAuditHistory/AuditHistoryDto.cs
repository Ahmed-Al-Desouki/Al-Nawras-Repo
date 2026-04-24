using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.AuditLogs.Queries.GetAuditHistory
{
    public record AuditHistoryDto(
        long Id,
        string TableName,
        string RecordId,
        string Action,
        int? PerformedByUserId,
        string? PerformedByName,
        string? OldValues,
        string? NewValues,
        string? IpAddress,
        DateTime CreatedAt
    );
}
