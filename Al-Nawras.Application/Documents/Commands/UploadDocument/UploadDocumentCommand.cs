using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Documents.Commands.UploadDocument
{
    public record UploadDocumentCommand(
       Guid DealId,
       Guid? ShipmentId,
       DocumentType DocumentType,
       string FileName,
       string MimeType,
       long FileSizeBytes,
       Stream FileStream,
       int UploadedByUserId
   );
}
