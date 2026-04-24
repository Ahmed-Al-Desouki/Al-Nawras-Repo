using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Reporting.Commands.CreateReportTemplate
{
    public record CreateReportTemplateCommand(
        string Name,
        string Slug,
        ReportTemplateCategory Category,
        string Description,
        List<ReportTemplateSheetDefinitionDto> Sheets,
        int CreatedByUserId);
}
