using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Documents.Commands.UploadDocument
{
    public class UploadDocumentHandler
    {
        private readonly IDealRepository _dealRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public UploadDocumentHandler(
            IDealRepository dealRepository,
            IFileStorageService fileStorageService,
            IApplicationDbContext context,
            IUnitOfWork unitOfWork)
        {
            _dealRepository = dealRepository;
            _fileStorageService = fileStorageService;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UploadDocumentCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!_dealRepository.Exists(command.DealId))
                return Result<Guid>.Failure($"Deal {command.DealId} not found.");

            // Validate file type
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

            // Validate file size — 20MB max
            const long maxBytes = 20 * 1024 * 1024;
            if (command.FileSizeBytes > maxBytes)
                return Result<Guid>.Failure("File size exceeds the 20MB limit.");

            // Save the file to storage — get back the relative path
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

            return Result<Guid>.Success(document.Id);
        }
    }
}
