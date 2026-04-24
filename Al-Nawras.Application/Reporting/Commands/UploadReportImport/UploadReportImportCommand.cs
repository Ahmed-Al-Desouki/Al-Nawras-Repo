namespace Al_Nawras.Application.Reporting.Commands.UploadReportImport
{
    public record UploadReportImportCommand(
        string? ReportTemplateId,
        string Name,
        string Description,
        string FileName,
        string MimeType,
        long FileSizeBytes,
        Stream FileStream,
        int UploadedByUserId);
}
