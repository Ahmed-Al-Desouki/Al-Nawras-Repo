using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.GetReportImportById
{
    public class GetReportImportByIdHandler
    {
        private readonly IApplicationDbContext _context;

        public GetReportImportByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ReportImportDetailDto>> Handle(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var import = await _context.ReportImports
                .AsNoTracking()
                .Include(i => i.ReportTemplate)
                .Include(i => i.ReviewedByUser)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (import is null)
                return Result<ReportImportDetailDto>.Failure("Imported workbook not found.");

            var workbook = JsonSerializer.Deserialize<WorkbookSnapshotDto>(import.WorkbookJson)
                           ?? new WorkbookSnapshotDto(import.SourceFileName, 0, 0, 0, []);

            var analysis = JsonSerializer.Deserialize<ImportedWorkbookAnalysisDto>(import.AnalysisJson)
                           ?? new ImportedWorkbookAnalysisDto([], [], [], new ImportLinkAnalysisDto([], [], [], []));

            return Result<ReportImportDetailDto>.Success(new ReportImportDetailDto(
                import.Id,
                import.ReportTemplateId,
                import.Name,
                import.Description,
                import.SourceFileName,
                import.SourceStoragePath,
                import.ReportTemplate?.Name,
                import.Status,
                import.ReviewedByUser is null ? null : $"{import.ReviewedByUser.FirstName} {import.ReviewedByUser.LastName}".Trim(),
                import.ReviewedAt,
                import.ReviewNotes,
                workbook,
                analysis,
                import.CreatedAt));
        }
    }
}
