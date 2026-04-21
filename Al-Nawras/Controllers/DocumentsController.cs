using Al_Nawras.Application.Documents.Commands.UploadDocument;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalOnly")]
    public class DocumentsController : ControllerBase
    {
        private readonly UploadDocumentHandler _uploadHandler;

        public DocumentsController(UploadDocumentHandler uploadHandler)
        {
            _uploadHandler = uploadHandler;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(20 * 1024 * 1024)]   // 20MB
        public async Task<IActionResult> Upload(
            [FromForm] Guid dealId,
            [FromForm] Guid? shipmentId,
            [FromForm] DocumentType documentType,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file was uploaded." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new UploadDocumentCommand(
                DealId: dealId,
                ShipmentId: shipmentId,
                DocumentType: documentType,
                FileName: file.FileName,
                MimeType: file.ContentType,
                FileSizeBytes: file.Length,
                FileStream: file.OpenReadStream(),
                UploadedByUserId: userId
            );

            var result = await _uploadHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(new { id = result.Value });
        }
    }
}
