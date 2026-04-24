using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Al_Nawras.Application.Reporting.Commands.ReviewReportImport
{
    public class ReviewReportImportHandler
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewReportImportHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ReviewReportImportCommand command,
            CancellationToken cancellationToken = default)
        {
            var import = await _context.ReportImports
                .FirstOrDefaultAsync(i => i.Id == command.ReportImportId, cancellationToken);

            if (import is null)
                return Result.Failure("Imported workbook not found.");

            if (command.Status == ReportImportStatus.PendingApproval)
                return Result.Failure("PendingApproval is not a valid review action.");

            if (command.Status == ReportImportStatus.Approved)
                import.Approve(command.ReviewedByUserId, command.ReviewNotes);
            else
                import.Reject(command.ReviewedByUserId, command.ReviewNotes);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
