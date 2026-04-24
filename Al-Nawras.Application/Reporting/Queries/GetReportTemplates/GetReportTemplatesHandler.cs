using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Queries.GetReportTemplates
{
    public class GetReportTemplatesHandler
    {
        private readonly IApplicationDbContext _context;

        public GetReportTemplatesHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReportTemplateDto>>> Handle(CancellationToken cancellationToken = default)
        {
            var templates = await _context.ReportTemplates
                .AsNoTracking()
                .OrderByDescending(t => t.IsSystem)
                .ThenBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var result = templates.Select(template =>
            {
                var definition = JsonSerializer.Deserialize<ReportTemplateDefinitionDto>(template.DefinitionJson)
                                 ?? new ReportTemplateDefinitionDto(template.Slug, template.Name, template.Description, template.Category, []);

                return new ReportTemplateDto(
                    template.Id,
                    template.Name,
                    template.Slug,
                    template.Category,
                    template.Description,
                    template.IsSystem,
                    definition.Sheets.Count,
                    template.CreatedAt);
            }).ToList();

            return Result<List<ReportTemplateDto>>.Success(result);
        }
    }
}
