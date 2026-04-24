using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.AuditLogs.Queries.GetAuditHistory
{
    public record GetAuditHistoryQuery(
        string TableName,
        string RecordId
    );
}
