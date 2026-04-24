using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Reporting.DTOs
{
    public record ReportTemplateDefinitionDto(
        string TemplateCode,
        string Name,
        string Description,
        ReportTemplateCategory Category,
        List<ReportTemplateSheetDefinitionDto> Sheets);

    public record ReportTemplateSheetDefinitionDto(
        string SheetName,
        string Purpose,
        List<ReportTemplateColumnDefinitionDto> Columns);

    public record ReportTemplateColumnDefinitionDto(
        string Header,
        string DataType,
        bool IsRequired,
        string ExampleValue,
        string Notes);

    public record ReportTemplateDto(
        Guid Id,
        string Name,
        string Slug,
        ReportTemplateCategory Category,
        string Description,
        bool IsSystem,
        int SheetCount,
        DateTime CreatedAt);

    public record ReportTemplateDetailDto(
        Guid Id,
        string Name,
        string Slug,
        ReportTemplateCategory Category,
        string Description,
        bool IsSystem,
        DateTime CreatedAt,
        ReportTemplateDefinitionDto Definition);

    public record WorkbookSheetSnapshotDto(
        string Name,
        int RowCount,
        int ColumnCount,
        List<string> Headers,
        List<List<string>> Rows);

    public record WorkbookSnapshotDto(
        string FileName,
        int WorksheetCount,
        int TotalRowCount,
        int TotalNonEmptyCellCount,
        List<WorkbookSheetSnapshotDto> Worksheets);

    public record ImportLinkAnalysisDto(
        List<string> MatchedDealNumbers,
        List<string> MatchedClientNames,
        List<string> MatchedShipmentNumbers,
        List<string> MatchedTrackingNumbers);

    public record ImportedWorkbookAnalysisDto(
        List<string> DetectedHeaders,
        List<string> NumericColumns,
        List<string> DateColumns,
        ImportLinkAnalysisDto Links);

    public record ImportedWorkbookParseResult(
        WorkbookSnapshotDto Snapshot,
        ImportedWorkbookAnalysisDto Analysis);

    public record ReportImportDto(
        Guid Id,
        Guid? ReportTemplateId,
        string Name,
        string Description,
        string SourceFileName,
        string? TemplateName,
        ReportImportStatus Status,
        string? ReviewedByName,
        DateTime? ReviewedAt,
        int WorksheetCount,
        int RowCount,
        int NonEmptyCellCount,
        DateTime CreatedAt);

    public record ReportImportDetailDto(
        Guid Id,
        Guid? ReportTemplateId,
        string Name,
        string Description,
        string SourceFileName,
        string SourceStoragePath,
        string? TemplateName,
        ReportImportStatus Status,
        string? ReviewedByName,
        DateTime? ReviewedAt,
        string ReviewNotes,
        WorkbookSnapshotDto Workbook,
        ImportedWorkbookAnalysisDto Analysis,
        DateTime CreatedAt);

    public record ReportingOverviewTemplateSummaryDto(
        string TemplateName,
        int ImportCount,
        int TotalRows,
        int TotalNonEmptyCells,
        int LinkedDeals,
        int LinkedClients,
        int LinkedShipments);

    public record ReportingOverviewDto(
        int TotalTemplates,
        int SystemTemplates,
        int CustomTemplates,
        int TotalImports,
        int PendingImports,
        int ApprovedImports,
        int RejectedImports,
        int ImportsLast30Days,
        int TotalRowsImported,
        int TotalWorksheetsImported,
        int TotalNonEmptyCellsImported,
        List<string> TopHeaders,
        ReportingLinkSummaryDto Links,
        List<ReportingOverviewTemplateSummaryDto> TemplateSummaries,
        List<ReportImportDto> RecentImports);

    public record ReportingLinkSummaryDto(
        int Deals,
        int Clients,
        int Shipments,
        int TrackingNumbers);
}
