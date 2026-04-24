using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Documents.Commands.UploadDocument
{
    public class UploadDocumentHandler
    {
        private readonly IDealRepository _dealRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IApplicationDbContext _context;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public UploadDocumentHandler(
            IDealRepository dealRepository,
            IFileStorageService fileStorageService,
            IApplicationDbContext context,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork)
        {
            _dealRepository = dealRepository;
            _fileStorageService = fileStorageService;
            _context = context;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UploadDocumentCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!_dealRepository.Exists(command.DealId))
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            var allowedMimeTypes = new[]
            {
                "application/pdf",
                "image/jpeg",
                "image/png",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-excel"
            };

            if (!allowedMimeTypes.Contains(command.MimeType.ToLower()))
                return Result<Guid>.Failure(
                    "File type not allowed. Accepted types: PDF, JPG, PNG, XLSX.");

            const long maxBytes = 20 * 1024 * 1024;
            if (command.FileSizeBytes > maxBytes)
                return Result<Guid>.Failure("File size exceeds the 20MB limit.");

            var folder = $"deals/{command.DealId}/documents";
            var storagePath = await _fileStorageService.SaveAsync(
                command.FileStream, command.FileName, folder, cancellationToken);

            var document = new Document(
                command.DealId,
                command.DocumentType,
                command.FileName,
                storagePath,
                command.FileSizeBytes,
                command.MimeType,
                command.UploadedByUserId,
                command.ShipmentId
            );

            await _context.Documents.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dealSnapshot = await _context.Deals
                .Where(d => d.Id == command.DealId)
                .Select(d => new { d.ClientId, d.AssignedSalesUserId, d.DealNumber })
                .FirstAsync(cancellationToken);

            await _notificationDispatcher.DispatchAsync(
                new WorkflowNotificationRequest(
                    Type: NotificationType.DocumentUploaded,
                    Title: "New document uploaded",
                    Body: $"Document {command.FileName} has been uploaded for deal {dealSnapshot.DealNumber}.",
                    RelatedEntityId: document.Id,
                    RelatedEntityType: nameof(Document),
                    UserIds: [dealSnapshot.AssignedSalesUserId],
                    RoleNames: ["operations", "sales", "admin"],
                    ClientId: dealSnapshot.ClientId),
                cancellationToken);

            return Result<Guid>.Success(document.Id);
        }
    }
}
