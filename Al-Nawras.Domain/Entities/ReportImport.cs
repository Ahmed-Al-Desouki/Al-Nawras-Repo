namespace Al_Nawras.Domain.Entities
{
    public class ReportImport
    {
        public Guid Id { get; private set; }
        public Guid? ReportTemplateId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string SourceFileName { get; private set; }
        public string SourceStoragePath { get; private set; }
        public string WorkbookJson { get; private set; }
        public string AnalysisJson { get; private set; }
        public int WorksheetCount { get; private set; }
        public int RowCount { get; private set; }
        public int NonEmptyCellCount { get; private set; }
        public Enums.ReportImportStatus Status { get; private set; }
        public int? ReviewedByUserId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string ReviewNotes { get; private set; }
        public int UploadedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public ReportTemplate? ReportTemplate { get; private set; }
        public User UploadedByUser { get; private set; }
        public User? ReviewedByUser { get; private set; }

        private ReportImport() { }

        public ReportImport(
            Guid? reportTemplateId,
            string name,
            string description,
            string sourceFileName,
            string sourceStoragePath,
            string workbookJson,
            string analysisJson,
            int worksheetCount,
            int rowCount,
            int nonEmptyCellCount,
            int uploadedByUserId)
        {
            Id = Guid.NewGuid();
            ReportTemplateId = reportTemplateId;
            Name = name;
            Description = description;
            SourceFileName = sourceFileName;
            SourceStoragePath = sourceStoragePath;
            WorkbookJson = workbookJson;
            AnalysisJson = analysisJson;
            WorksheetCount = worksheetCount;
            RowCount = rowCount;
            NonEmptyCellCount = nonEmptyCellCount;
            Status = Enums.ReportImportStatus.PendingApproval;
            ReviewNotes = string.Empty;
            UploadedByUserId = uploadedByUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Approve(int reviewedByUserId, string? reviewNotes = null)
        {
            Status = Enums.ReportImportStatus.Approved;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.UtcNow;
            ReviewNotes = reviewNotes?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(int reviewedByUserId, string? reviewNotes = null)
        {
            Status = Enums.ReportImportStatus.Rejected;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.UtcNow;
            ReviewNotes = reviewNotes?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
