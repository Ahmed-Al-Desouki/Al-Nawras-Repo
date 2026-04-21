using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public Guid DealId { get; private set; }
        public Guid? ShipmentId { get; private set; }
        public DocumentType DocumentType { get; private set; }
        public string FileName { get; private set; }
        public string StoragePath { get; private set; }
        public long FileSizeBytes { get; private set; }
        public string MimeType { get; private set; }
        public int UploadedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Deal Deal { get; private set; }
        public Shipment Shipment { get; private set; }
        public User UploadedByUser { get; private set; }

        private Document() { }

        public Document(Guid dealId, DocumentType documentType, string fileName,
            string storagePath, long fileSizeBytes, string mimeType,
            int uploadedByUserId, Guid? shipmentId = null)
        {
            Id = Guid.NewGuid();
            DealId = dealId;
            ShipmentId = shipmentId;
            DocumentType = documentType;
            FileName = fileName;
            StoragePath = storagePath;
            FileSizeBytes = fileSizeBytes;
            MimeType = mimeType;
            UploadedByUserId = uploadedByUserId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
