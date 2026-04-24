using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.GetReportTemplateById
{
    public class GetReportTemplateByIdHandler
    {
        private readonly IApplicationDbContext _context;

        public GetReportTemplateByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ReportTemplateDetailDto>> Handle(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var template = await _context.ReportTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (template is null)
                return Result<ReportTemplateDetailDto>.Failure("Report template not found.");

            var definition = JsonSerializer.Deserialize<ReportTemplateDefinitionDto>(template.DefinitionJson)
                             ?? new ReportTemplateDefinitionDto(template.Slug, template.Name, template.Description, template.Category, []);

            return Result<ReportTemplateDetailDto>.Success(new ReportTemplateDetailDto(
                template.Id,
                template.Name,
                template.Slug,
                template.Category,
                template.Description,
                template.IsSystem,
                template.CreatedAt,
                definition));
        }
    }
}
