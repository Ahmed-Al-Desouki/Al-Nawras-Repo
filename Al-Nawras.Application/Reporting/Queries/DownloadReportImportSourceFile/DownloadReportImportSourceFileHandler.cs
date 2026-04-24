using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reports.Queries.ExportReport;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.DownloadReportImportSourceFile
{
    public class DownloadReportImportSourceFileHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public DownloadReportImportSourceFileHandler(
            IApplicationDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<ExcelFileResult>> Handle(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var import = await _context.ReportImports
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (import is null)
                return Result<ExcelFileResult>.Failure("Imported workbook not found.");

            await using var stream = await _fileStorageService.OpenReadAsync(
                import.SourceStoragePath,
                cancellationToken);

            if (stream is null)
                return Result<ExcelFileResult>.Failure("The uploaded source file could not be found on storage.");

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            return Result<ExcelFileResult>.Success(new ExcelFileResult(
                memoryStream.ToArray(),
                import.SourceFileName,
                GetContentType(import.SourceFileName)));
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".xls" => "application/vnd.ms-excel",
                _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }
    }
}
