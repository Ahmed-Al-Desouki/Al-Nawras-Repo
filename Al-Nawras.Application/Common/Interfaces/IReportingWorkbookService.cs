using Al_Nawras.Application.Reporting.DTOs;

namespace Al_Nawras.Application.Common.Interfaces
{
    public interface IReportingWorkbookService
    {
        byte[] GenerateTemplateWorkbook(ReportTemplateDefinitionDto definition);
        ImportedWorkbookParseResult ParseWorkbook(Stream fileStream, string fileName);
    }
}
