using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Application.Reports.Queries.ExportReport;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.DownloadReportTemplate
{
    public class DownloadReportTemplateHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IReportingWorkbookService _reportingWorkbookService;

        public DownloadReportTemplateHandler(
            IApplicationDbContext context,
            IReportingWorkbookService reportingWorkbookService)
        {
            _context = context;
            _reportingWorkbookService = reportingWorkbookService;
        }

        public async Task<Result<ExcelFileResult>> Handle(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var template = await _context.ReportTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (template is null)
                return Result<ExcelFileResult>.Failure("Report template not found.");

            var definition = JsonSerializer.Deserialize<ReportTemplateDefinitionDto>(template.DefinitionJson)
                             ?? new ReportTemplateDefinitionDto(template.Slug, template.Name, template.Description, template.Category, []);

            var bytes = _reportingWorkbookService.GenerateTemplateWorkbook(definition);
            var fileName = $"{template.Slug}-template.xlsx";

            return Result<ExcelFileResult>.Success(new ExcelFileResult(
                bytes,
                fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        }
    }
}
