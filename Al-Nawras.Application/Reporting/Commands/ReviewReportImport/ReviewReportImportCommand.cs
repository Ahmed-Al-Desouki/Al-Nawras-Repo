using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Reporting.Commands.ReviewReportImport
{
    public record ReviewReportImportCommand(
        Guid ReportImportId,
        ReportImportStatus Status,
        string? ReviewNotes,
        int ReviewedByUserId);
}
