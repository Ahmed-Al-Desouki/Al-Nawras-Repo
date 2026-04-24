using System.Text.Json;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Reporting.DTOs;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Commands.CreateReportTemplate
{
    public class CreateReportTemplateHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReportTemplateHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateReportTemplateCommand command,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return Result<Guid>.Failure("Template name is required.");

            if (command.Sheets is null || command.Sheets.Count == 0)
                return Result<Guid>.Failure("At least one worksheet is required.");

            var slug = command.Slug.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(slug))
                return Result<Guid>.Failure("Template slug is required.");

            var exists = await _context.ReportTemplates
                .AnyAsync(t => t.Slug == slug, cancellationToken);

            if (exists)
                return Result<Guid>.Failure($"A template with slug '{slug}' already exists.");

            var definition = new ReportTemplateDefinitionDto(
                TemplateCode: slug,
                Name: command.Name.Trim(),
                Description: command.Description?.Trim() ?? string.Empty,
                Category: command.Category,
                Sheets: command.Sheets);

            var template = new ReportTemplate(
                definition.Name,
                slug,
                command.Category,
                definition.Description,
                JsonSerializer.Serialize(definition),
                isSystem: false,
                createdByUserId: command.CreatedByUserId);

            await _context.ReportTemplates.AddAsync(template, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(template.Id);
        }
    }
}
