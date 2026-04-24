using Al_Nawras.Application.ClientPortal.Queries.GetMyDealDetail;
using Al_Nawras.Application.ClientPortal.Queries.GetMyDeals;
using Al_Nawras.Application.ClientPortal.Queries.GetMyDocuments;
using Al_Nawras.Application.ClientPortal.Queries.GetMyPayments;
using Al_Nawras.Application.ClientPortal.Queries.GetMyShipments;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/portal")]
    [Authorize(Policy = "ClientOnly")]   // ONLY clients hit these endpoints
    public class ClientPortalController : BaseClientController
    {
        private readonly GetMyDealsHandler _dealsHandler;
        private readonly GetMyDealDetailHandler _dealDetailHandler;
        private readonly GetMyShipmentsHandler _shipmentsHandler;
        private readonly GetMyPaymentsHandler _paymentsHandler;
        private readonly GetMyDocumentsHandler _documentsHandler;
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ClientPortalController(
            GetMyDealsHandler dealsHandler,
            GetMyDealDetailHandler dealDetailHandler,
            GetMyShipmentsHandler shipmentsHandler,
            GetMyPaymentsHandler paymentsHandler,
            GetMyDocumentsHandler documentsHandler,
            IApplicationDbContext context,
            IConfiguration configuration)
        {
            _dealsHandler = dealsHandler;
            _dealDetailHandler = dealDetailHandler;
            _shipmentsHandler = shipmentsHandler;
            _paymentsHandler = paymentsHandler;
            _documentsHandler = documentsHandler;
            _context = context;
            _configuration = configuration;
        }

        // ── Deals ──────────────────────────────────────────────────────────────────

        /// <summary>Get all deals belonging to this client</summary>
        [HttpGet("deals")]
        public async Task<IActionResult> GetMyDeals(
            [FromQuery] DealStatus? status,
            CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var result = await _dealsHandler.Handle(
                new GetMyDealsQuery(clientId.Value, status), cancellationToken);

            return Ok(result.Value);
        }

        /// <summary>Get full detail of a single deal including shipments, payments, documents</summary>
        [HttpGet("deals/{id:guid}")]
        public async Task<IActionResult> GetMyDealDetail(Guid id, CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var result = await _dealDetailHandler.Handle(
                new GetMyDealDetailQuery(id, clientId.Value), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        // ── Shipments ──────────────────────────────────────────────────────────────

        /// <summary>Get all shipments across all deals for this client</summary>
        [HttpGet("shipments")]
        public async Task<IActionResult> GetMyShipments(CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var result = await _shipmentsHandler.Handle(
                new GetMyShipmentsQuery(clientId.Value), cancellationToken);

            return Ok(result.Value);
        }

        // ── Payments ───────────────────────────────────────────────────────────────

        /// <summary>Get all payments for this client, optionally filtered by status</summary>
        [HttpGet("payments")]
        public async Task<IActionResult> GetMyPayments(
            [FromQuery] PaymentStatus? status,
            CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var result = await _paymentsHandler.Handle(
                new GetMyPaymentsQuery(clientId.Value, status), cancellationToken);

            return Ok(result.Value);
        }

        // ── Documents ──────────────────────────────────────────────────────────────

        /// <summary>Get all documents, optionally filtered by a specific deal</summary>
        [HttpGet("documents")]
        public async Task<IActionResult> GetMyDocuments(
            [FromQuery] Guid? dealId,
            CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var result = await _documentsHandler.Handle(
                new GetMyDocumentsQuery(clientId.Value, dealId), cancellationToken);

            return Ok(result.Value);
        }

        /// <summary>
        /// Download a document by ID.
        /// Validates ownership before streaming the file —
        /// a client cannot download another client's document by guessing the ID.
        /// </summary>
        [HttpGet("documents/{id:guid}/download")]
        public async Task<IActionResult> DownloadDocument(Guid id, CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            // Load document and verify it belongs to this client via the Deal
            var document = await _context.Documents
                .Where(d => d.Id == id && d.Deal.ClientId == clientId.Value)
                .Select(d => new { d.FileName, d.StoragePath, d.MimeType })
                .FirstOrDefaultAsync(cancellationToken);

            if (document is null)
                return NotFound(new { error = "Document not found." });

            var basePath = _configuration["FileStorage:BasePath"]!;
            var absolutePath = Path.Combine(basePath, document.StoragePath);

            if (!System.IO.File.Exists(absolutePath))
                return NotFound(new { error = "File no longer exists on the server." });

            var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
            var contentType = document.MimeType ?? GetContentType(document.FileName);

            return File(stream, contentType, document.FileName);
        }

        // ── Summary dashboard for the client ──────────────────────────────────────

        /// <summary>Lightweight summary — active deals, pending payments, in-transit shipments</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var clientId = TryGetClientId();
            if (clientId is null) return ClientIdMissing();

            var cid = clientId.Value;

            var activeDealCount = await _context.Deals
                .CountAsync(d => d.ClientId == cid
                              && d.Status != DealStatus.Closed, cancellationToken);

            var pendingPayments = await _context.Payments
                .Where(p => p.Deal.ClientId == cid
                         && p.Status != PaymentStatus.FullyPaid)
                .Select(p => new { p.Amount, p.Currency, p.DueDate, p.Status })
                .ToListAsync(cancellationToken);

            var inTransitShipments = await _context.Shipments
                .CountAsync(s => s.Deal.ClientId == cid
                              && (s.Status == ShipmentStatus.InTransit
                               || s.Status == ShipmentStatus.AtCustoms), cancellationToken);

            var recentDeals = await _context.Deals
                .Where(d => d.ClientId == cid)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .Select(d => new
                {
                    d.Id,
                    d.DealNumber,
                    Status = d.Status.ToString(),
                    d.Commodity,
                    d.TotalValue,
                    d.Currency,
                    d.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var summary = new
            {
                ActiveDeals = activeDealCount,
                InTransitShipments = inTransitShipments,
                PendingPaymentsCount = pendingPayments.Count,
                OverduePaymentsCount = pendingPayments.Count(p =>
                                        p.DueDate < DateTime.UtcNow &&
                                        p.Status.ToString() != "FullyPaid"),
                RecentDeals = recentDeals
            };

            return Ok(summary);
        }

        private static string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(fileName, out var contentType)
                ? contentType
                : "application/octet-stream";
        }
    }
}
