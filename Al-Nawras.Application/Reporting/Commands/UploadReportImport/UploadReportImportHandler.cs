using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Commands.UploadReportImport
{
    public class UploadReportImportHandler
    {
        private static readonly string[] AllowedMimeTypes =
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel"
        };

        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IReportingWorkbookService _reportingWorkbookService;

        public UploadReportImportHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IReportingWorkbookService reportingWorkbookService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _reportingWorkbookService = reportingWorkbookService;
        }

        public async Task<Result<Guid>> Handle(
            UploadReportImportCommand command,
            CancellationToken cancellationToken = default)
        {
            Guid? reportTemplateId = null;

            if (command.FileSizeBytes <= 0)
                return Result<Guid>.Failure("The uploaded Excel file is empty.");

            if (!AllowedMimeTypes.Contains(command.MimeType.ToLowerInvariant()))
                return Result<Guid>.Failure("Only Excel files (.xlsx, .xls) are supported.");

            const long maxBytes = 25 * 1024 * 1024;
            if (command.FileSizeBytes > maxBytes)
                return Result<Guid>.Failure("Excel file size exceeds the 25MB limit.");

            if (!string.IsNullOrWhiteSpace(command.ReportTemplateId))
            {
                if (!Guid.TryParse(command.ReportTemplateId, out var parsedTemplateId))
                    return Result<Guid>.Failure("The selected report template id is not a valid GUID.");

                reportTemplateId = parsedTemplateId;

                var templateExists = await _context.ReportTemplates
                    .AnyAsync(t => t.Id == parsedTemplateId, cancellationToken);

                if (!templateExists)
                    return Result<Guid>.Failure("The selected report template was not found.");
            }

            using var memoryStream = new MemoryStream();
            await command.FileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var parseResult = _reportingWorkbookService.ParseWorkbook(memoryStream, command.FileName);
            var enrichedAnalysis = await EnrichLinksAsync(parseResult, cancellationToken);

            memoryStream.Position = 0;
            var storagePath = await _fileStorageService.SaveAsync(
                memoryStream,
                command.FileName,
                "reporting/imports",
                cancellationToken);

            var import = new ReportImport(
                reportTemplateId,
                string.IsNullOrWhiteSpace(command.Name)
                    ? Path.GetFileNameWithoutExtension(command.FileName)
                    : command.Name.Trim(),
                command.Description?.Trim() ?? string.Empty,
                command.FileName,
                storagePath,
                JsonSerializer.Serialize(parseResult.Snapshot),
                JsonSerializer.Serialize(enrichedAnalysis),
                parseResult.Snapshot.WorksheetCount,
                parseResult.Snapshot.TotalRowCount,
                parseResult.Snapshot.TotalNonEmptyCellCount,
                command.UploadedByUserId);

            await _context.ReportImports.AddAsync(import, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(import.Id);
        }

        private async Task<ImportedWorkbookAnalysisDto> EnrichLinksAsync(
            ImportedWorkbookParseResult parseResult,
            CancellationToken cancellationToken)
        {
            var workbookValues = parseResult.Snapshot.Worksheets
                .SelectMany(w => w.Rows)
                .SelectMany(r => r)
                .Select(Normalize)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToHashSet();

            var dealNumbers = await _context.Deals
                .Select(d => d.DealNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var clientNames = await _context.Clients
                .Select(c => c.CompanyName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var shipmentNumbers = await _context.Shipments
                .Select(s => s.ShipmentNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var trackingNumbers = await _context.Shipments
                .Where(s => s.TrackingNumber != string.Empty)
                .Select(s => s.TrackingNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var links = new ImportLinkAnalysisDto(
                MatchedDealNumbers: dealNumbers.Where(d => workbookValues.Contains(Normalize(d))).Distinct().ToList(),
                MatchedClientNames: clientNames.Where(c => workbookValues.Contains(Normalize(c))).Distinct().ToList(),
                MatchedShipmentNumbers: shipmentNumbers.Where(s => workbookValues.Contains(Normalize(s))).Distinct().ToList(),
                MatchedTrackingNumbers: trackingNumbers.Where(t => workbookValues.Contains(Normalize(t))).Distinct().ToList());

            return parseResult.Analysis with { Links = links };
        }

        private static string Normalize(string? value)
            => string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
    }
}
