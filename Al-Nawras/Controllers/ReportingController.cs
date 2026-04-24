using System.Security.Claims;
using Al_Nawras.Application.Reporting.Commands.CreateReportTemplate;
using Al_Nawras.Application.Reporting.Commands.ReviewReportImport;
using Al_Nawras.Application.Reporting.Commands.UploadReportImport;
using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Application.Reporting.Queries.DownloadReportImportSourceFile;
using Al_Nawras.Application.Reporting.Queries.DownloadReportTemplate;
using Al_Nawras.Application.Reporting.Queries.GetReportImportById;
using Al_Nawras.Application.Reporting.Queries.GetReportImports;
using Al_Nawras.Application.Reporting.Queries.GetReportTemplateById;
using Al_Nawras.Application.Reporting.Queries.GetReportTemplates;
using Al_Nawras.Application.Reporting.Queries.GetReportingOverview;
using Al_Nawras.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/reporting")]
    [Authorize(Policy = "InternalOnly")]
    public class ReportingController : ControllerBase
    {
        private readonly GetReportTemplatesHandler _getTemplatesHandler;
        private readonly GetReportTemplateByIdHandler _getTemplateByIdHandler;
        private readonly CreateReportTemplateHandler _createTemplateHandler;
        private readonly ReviewReportImportHandler _reviewReportImportHandler;
        private readonly DownloadReportImportSourceFileHandler _downloadReportImportSourceFileHandler;
        private readonly DownloadReportTemplateHandler _downloadTemplateHandler;
        private readonly UploadReportImportHandler _uploadReportImportHandler;
        private readonly GetReportImportsHandler _getReportImportsHandler;
        private readonly GetReportImportByIdHandler _getReportImportByIdHandler;
        private readonly GetReportingOverviewHandler _getReportingOverviewHandler;

        public ReportingController(
            GetReportTemplatesHandler getTemplatesHandler,
            GetReportTemplateByIdHandler getTemplateByIdHandler,
            CreateReportTemplateHandler createTemplateHandler,
            ReviewReportImportHandler reviewReportImportHandler,
            DownloadReportImportSourceFileHandler downloadReportImportSourceFileHandler,
            DownloadReportTemplateHandler downloadTemplateHandler,
            UploadReportImportHandler uploadReportImportHandler,
            GetReportImportsHandler getReportImportsHandler,
            GetReportImportByIdHandler getReportImportByIdHandler,
            GetReportingOverviewHandler getReportingOverviewHandler)
        {
            _getTemplatesHandler = getTemplatesHandler;
            _getTemplateByIdHandler = getTemplateByIdHandler;
            _createTemplateHandler = createTemplateHandler;
            _reviewReportImportHandler = reviewReportImportHandler;
            _downloadReportImportSourceFileHandler = downloadReportImportSourceFileHandler;
            _downloadTemplateHandler = downloadTemplateHandler;
            _uploadReportImportHandler = uploadReportImportHandler;
            _getReportImportsHandler = getReportImportsHandler;
            _getReportImportByIdHandler = getReportImportByIdHandler;
            _getReportingOverviewHandler = getReportingOverviewHandler;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
        {
            var result = await _getTemplatesHandler.Handle(cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("templates/{id:guid}")]
        public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
        {
            var result = await _getTemplateByIdHandler.Handle(id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate(
            [FromBody] CreateTemplateRequest request,
            CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _createTemplateHandler.Handle(new CreateReportTemplateCommand(
                request.Name,
                request.Slug,
                request.Category,
                request.Description,
                request.Sheets,
                userId), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetTemplate), new { id = result.Value }, new { id = result.Value });
        }

        [HttpGet("templates/{id:guid}/download")]
        public async Task<IActionResult> DownloadTemplate(Guid id, CancellationToken cancellationToken)
        {
            var result = await _downloadTemplateHandler.Handle(id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return File(result.Value.Bytes, result.Value.ContentType, result.Value.FileName);
        }

        [HttpPost("imports/upload-external-file")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> UploadExternalImport(
            [FromForm] UploadExternalReportImportRequest request,
            CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return BadRequest(new { error = "No Excel file was uploaded." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _uploadReportImportHandler.Handle(new UploadReportImportCommand(
                null,
                request.Name ?? string.Empty,
                request.Description ?? string.Empty,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                request.File.OpenReadStream(),
                userId), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(new { id = result.Value });
        }

        [HttpPost("imports/upload-template-file")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> UploadTemplateImport(
            [FromForm] UploadTemplateReportImportRequest request,
            CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return BadRequest(new { error = "No Excel file was uploaded." });

            if (string.IsNullOrWhiteSpace(request.ReportTemplateId))
                return BadRequest(new { error = "reportTemplateId is required for template-based imports." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _uploadReportImportHandler.Handle(new UploadReportImportCommand(
                request.ReportTemplateId,
                request.Name ?? string.Empty,
                request.Description ?? string.Empty,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                request.File.OpenReadStream(),
                userId), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(new { id = result.Value });
        }

        [HttpPost("imports/{id:guid}/review")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ReviewImport(
            Guid id,
            [FromBody] ReviewReportImportRequest request,
            CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _reviewReportImportHandler.Handle(new ReviewReportImportCommand(
                id,
                request.Status,
                request.ReviewNotes,
                userId), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }

        [HttpGet("imports")]
        public async Task<IActionResult> GetImports(CancellationToken cancellationToken)
        {
            var result = await _getReportImportsHandler.Handle(cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("imports/{id:guid}")]
        public async Task<IActionResult> GetImport(Guid id, CancellationToken cancellationToken)
        {
            var result = await _getReportImportByIdHandler.Handle(id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("imports/{id:guid}/download-source-file")]
        public async Task<IActionResult> DownloadImportSourceFile(Guid id, CancellationToken cancellationToken)
        {
            var result = await _downloadReportImportSourceFileHandler.Handle(id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return File(result.Value.Bytes, result.Value.ContentType, result.Value.FileName);
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
        {
            var result = await _getReportingOverviewHandler.Handle(cancellationToken);
            return Ok(result.Value);
        }
    }

    public record CreateTemplateRequest(
        string Name,
        string Slug,
        ReportTemplateCategory Category,
        string Description,
        List<ReportTemplateSheetDefinitionDto> Sheets);

    public record UploadExternalReportImportRequest(
        string? Name,
        string? Description,
        IFormFile File);

    public record UploadTemplateReportImportRequest(
        string ReportTemplateId,
        string? Name,
        string? Description,
        IFormFile File);

    public record ReviewReportImportRequest(
        ReportImportStatus Status,
        string? ReviewNotes);
}
