using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Al_Nawras.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Al_Nawras.Application.Jobs
{
    public class OverduePaymentJob : IOverduePaymentJob
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OverduePaymentJob> _logger;

        public OverduePaymentJob(
            IApplicationDbContext context,
            INotificationDispatcher notificationDispatcher,
            IUnitOfWork unitOfWork,
            ILogger<OverduePaymentJob> logger)
        {
            _context = context;
            _notificationDispatcher = notificationDispatcher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OverduePaymentJobResult> RunAsync(
            CancellationToken cancellationToken = default)
        {
            var startedAt = DateTime.UtcNow;
            var errors = new List<string>();
            var markedCount = 0;
            var notificationCount = 0;

            _logger.LogInformation("OverduePaymentJob started at {StartedAt}", startedAt);

            var overduePayments = await _context.Payments
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Client)
                .Where(p => p.Status != PaymentStatus.FullyPaid
                         && p.Status != PaymentStatus.Overdue
                         && p.DueDate < DateTime.UtcNow)
                .AsTracking()
                .ToListAsync(cancellationToken);

            if (overduePayments.Count == 0)
            {
                _logger.LogInformation("OverduePaymentJob: no payments to process.");

                return new OverduePaymentJobResult(
                    ProcessedCount: 0,
                    MarkedOverdueCount: 0,
                    NotificationsSentCount: 0,
                    Errors: errors,
                    RanAt: startedAt,
                    Duration: DateTime.UtcNow - startedAt
                );
            }

            var groupedByDeal = overduePayments.GroupBy(p => p.DealId).ToList();

            foreach (var payment in overduePayments)
            {
                try
                {
                    payment.MarkAsOverdue();
                    markedCount++;
                }
                catch (Exception ex)
                {
                    var error = $"Failed to mark payment {payment.PaymentReference}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogWarning(error);
                }
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var error = $"SaveChanges failed before notifications: {ex.Message}";
                errors.Add(error);
                _logger.LogError(ex, "OverduePaymentJob: SaveChanges failed before notifications.");

                return new OverduePaymentJobResult(
                    ProcessedCount: overduePayments.Count,
                    MarkedOverdueCount: markedCount,
                    NotificationsSentCount: notificationCount,
                    Errors: errors,
                    RanAt: startedAt,
                    Duration: DateTime.UtcNow - startedAt
                );
            }

            foreach (var dealGroup in groupedByDeal)
            {
                var firstPayment = dealGroup.First();
                var deal = firstPayment.Deal;
                var clientName = deal?.Client?.Name ?? "Unknown client";
                var dealNumber = deal?.DealNumber ?? "Unknown deal";
                var paymentCount = dealGroup.Count();
                var totalOverdue = dealGroup.Sum(p => p.AmountUSD);

                try
                {
                    await _notificationDispatcher.DispatchAsync(
                        new WorkflowNotificationRequest(
                            Type: NotificationType.PaymentOverdue,
                            Title: $"{paymentCount} overdue payment{(paymentCount > 1 ? "s" : "")} on {dealNumber}",
                            Body: $"Client: {clientName} | Total overdue: ${totalOverdue:N2} USD | {paymentCount} payment{(paymentCount > 1 ? "s" : "")} past due date.",
                            RelatedEntityId: dealGroup.Key,
                            RelatedEntityType: nameof(Deal),
                            UserIds: deal is null ? Array.Empty<int>() : [deal.AssignedSalesUserId],
                            RoleNames: ["accounts", "admin"],
                            ClientId: deal?.ClientId),
                        cancellationToken);

                    notificationCount++;
                }
                catch (Exception ex)
                {
                    var error = $"Failed to create notification for deal {dealGroup.Key}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogWarning(error);
                }
            }

            if (markedCount > 0)
            {
                var auditLog = new AuditLog(
                    tableName: "Payments",
                    recordId: "batch",
                    action: AuditAction.Update,
                    performedByUserId: null,
                    oldValues: string.Empty,
                    newValues: $"{{\"markedOverdue\":{markedCount},\"ranAt\":\"{startedAt:O}\"}}",
                    ipAddress: "system"
                );

                await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "OverduePaymentJob committed: {Marked} marked overdue, {Notifications} notification batches sent.",
                    markedCount,
                    notificationCount);
            }
            catch (Exception ex)
            {
                var error = $"SaveChanges failed: {ex.Message}";
                errors.Add(error);
                _logger.LogError(ex, "OverduePaymentJob: SaveChanges failed.");
            }

            var result = new OverduePaymentJobResult(
                ProcessedCount: overduePayments.Count,
                MarkedOverdueCount: markedCount,
                NotificationsSentCount: notificationCount,
                Errors: errors,
                RanAt: startedAt,
                Duration: DateTime.UtcNow - startedAt
            );

            _logger.LogInformation("OverduePaymentJob finished: {Summary}", result);
            return result;
        }
    }
}
