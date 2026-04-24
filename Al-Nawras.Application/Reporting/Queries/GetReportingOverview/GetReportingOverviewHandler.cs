using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.GetReportingOverview
{
    public class GetReportingOverviewHandler
    {
        private readonly IApplicationDbContext _context;

        public GetReportingOverviewHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ReportingOverviewDto>> Handle(CancellationToken cancellationToken = default)
        {
            var templates = await _context.ReportTemplates
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var imports = await _context.ReportImports
                .AsNoTracking()
                .Include(i => i.ReportTemplate)
                .Include(i => i.ReviewedByUser)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var approvedImports = imports
                .Where(i => i.Status == Domain.Enums.ReportImportStatus.Approved)
                .ToList();

            var analyses = approvedImports.Select(import =>
                JsonSerializer.Deserialize<ImportedWorkbookAnalysisDto>(import.AnalysisJson)
                ?? new ImportedWorkbookAnalysisDto([], [], [], new ImportLinkAnalysisDto([], [], [], [])))
                .ToList();

            var topHeaders = analyses
                .SelectMany(a => a.DetectedHeaders)
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .GroupBy(h => h.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Take(10)
                .Select(g => g.First())
                .ToList();

            var templateSummaries = approvedImports
                .GroupBy(i => i.ReportTemplate?.Name ?? "Ad-hoc uploads")
                .Select(group =>
                {
                    var groupAnalyses = group.Select(import =>
                        JsonSerializer.Deserialize<ImportedWorkbookAnalysisDto>(import.AnalysisJson)
                        ?? new ImportedWorkbookAnalysisDto([], [], [], new ImportLinkAnalysisDto([], [], [], [])))
                        .ToList();

                    return new ReportingOverviewTemplateSummaryDto(
                        group.Key,
                        group.Count(),
                        group.Sum(i => i.RowCount),
                        group.Sum(i => i.NonEmptyCellCount),
                        groupAnalyses.Sum(a => a.Links.MatchedDealNumbers.Count),
                        groupAnalyses.Sum(a => a.Links.MatchedClientNames.Count),
                        groupAnalyses.Sum(a => a.Links.MatchedShipmentNumbers.Count + a.Links.MatchedTrackingNumbers.Count));
                })
                .OrderByDescending(s => s.ImportCount)
                .ThenBy(s => s.TemplateName)
                .ToList();

            var recentImports = imports.Take(10).Select(i => new ReportImportDto(
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
                i.CreatedAt)).ToList();

            var dto = new ReportingOverviewDto(
                TotalTemplates: templates.Count,
                SystemTemplates: templates.Count(t => t.IsSystem),
                CustomTemplates: templates.Count(t => !t.IsSystem),
                TotalImports: imports.Count,
                PendingImports: imports.Count(i => i.Status == Domain.Enums.ReportImportStatus.PendingApproval),
                ApprovedImports: approvedImports.Count,
                RejectedImports: imports.Count(i => i.Status == Domain.Enums.ReportImportStatus.Rejected),
                ImportsLast30Days: imports.Count(i => i.CreatedAt >= thirtyDaysAgo),
                TotalRowsImported: approvedImports.Sum(i => i.RowCount),
                TotalWorksheetsImported: approvedImports.Sum(i => i.WorksheetCount),
                TotalNonEmptyCellsImported: approvedImports.Sum(i => i.NonEmptyCellCount),
                TopHeaders: topHeaders,
                Links: new ReportingLinkSummaryDto(
                    Deals: analyses.Sum(a => a.Links.MatchedDealNumbers.Count),
                    Clients: analyses.Sum(a => a.Links.MatchedClientNames.Count),
                    Shipments: analyses.Sum(a => a.Links.MatchedShipmentNumbers.Count),
                    TrackingNumbers: analyses.Sum(a => a.Links.MatchedTrackingNumbers.Count)),
                TemplateSummaries: templateSummaries,
                RecentImports: recentImports);

            return Result<ReportingOverviewDto>.Success(dto);
        }
    }
}
