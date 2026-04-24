using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.GetReportImports
{
    public class GetReportImportsHandler
    {
        private readonly IApplicationDbContext _context;

        public GetReportImportsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReportImportDto>>> Handle(CancellationToken cancellationToken = default)
        {
            var imports = await _context.ReportImports
                .AsNoTracking()
                .Include(i => i.ReportTemplate)
                .Include(i => i.ReviewedByUser)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);

            return Result<List<ReportImportDto>>.Success(imports.Select(i => new ReportImportDto(
                i.Id,
                i.ReportTemplateId,
                i.Name,
                i.Description,
                i.SourceFileName,
                i.ReportTemplate?.Name,
                i.Status,
                i.ReviewedByUser is null ? null : $"{i.ReviewedByUser.FirstName} {i.ReviewedByUser.LastName}".Trim(),
                i.ReviewedAt,
                i.WorksheetCount,
                i.RowCount,
                i.NonEmptyCellCount,
                i.CreatedAt)).ToList());
        }
    }
}
