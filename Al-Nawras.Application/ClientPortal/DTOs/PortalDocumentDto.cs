using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.DTOs
{
    public record PortalDocumentDto(
        Guid Id,
        string DocumentType,
        string FileName,
        long FileSizeBytes,
        string MimeType,
        DateTime CreatedAt
    );
}
