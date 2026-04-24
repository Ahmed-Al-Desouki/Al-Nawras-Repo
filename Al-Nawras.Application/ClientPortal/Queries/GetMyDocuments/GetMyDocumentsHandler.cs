using Al_Nawras.Application.ClientPortal.DTOs;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.ClientPortal.Queries.GetMyDocuments
{
    public class GetMyDocumentsHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMyDocumentsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PortalDocumentDto>>> Handle(
            GetMyDocumentsQuery query,
            CancellationToken cancellationToken = default)
        {
            var docsQuery = _context.Documents
                .Where(d => d.Deal.ClientId == query.ClientId);  // ownership via Deal

            if (query.DealId.HasValue)
                docsQuery = docsQuery.Where(d => d.DealId == query.DealId.Value);

            var documents = await docsQuery
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new PortalDocumentDto(
                    d.Id,
                    d.DocumentType.ToString(),
                    d.FileName,
                    d.FileSizeBytes,
                    d.MimeType,
                    d.CreatedAt
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<List<PortalDocumentDto>>.Success(documents);
        }
    }
}
