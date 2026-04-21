using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Documents.DTOs
{
    public record DocumentDto(
        Guid Id,
        Guid DealId,
        Guid? ShipmentId,
        DocumentType DocumentType,
        string DocumentTypeLabel,
        string FileName,
        string StoragePath,
        long FileSizeBytes,
        string MimeType,
        int UploadedByUserId,
        string UploadedByName,
        DateTime CreatedAt
    );
}
