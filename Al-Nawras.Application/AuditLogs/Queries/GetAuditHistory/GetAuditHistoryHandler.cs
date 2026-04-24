using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.AuditLogs.Queries.GetAuditHistory
{

    public class GetAuditHistoryHandler
    {
        private readonly IApplicationDbContext _context;

        public GetAuditHistoryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AuditHistoryDto>>> Handle(
            GetAuditHistoryQuery query,
            CancellationToken cancellationToken = default)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.TableName == query.TableName
                         && a.RecordId == query.RecordId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AuditHistoryDto(
                    a.Id,
                    a.TableName,
                    a.RecordId,
                    a.Action.ToString(),
                    a.PerformedByUserId,
                    a.PerformedByUser != null
                        ? $"{a.PerformedByUser.FirstName} {a.PerformedByUser.LastName}"
                        : "System",
                    a.OldValues,
                    a.NewValues,
                    a.IpAddress,
                    a.CreatedAt
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<List<AuditHistoryDto>>.Success(logs);
        }
    }
}
