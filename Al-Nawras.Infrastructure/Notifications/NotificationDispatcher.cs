using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Notifications;
using Al_Nawras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Al_Nawras.Infrastructure.Notifications
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IApplicationDbContext context,
            IEmailSender emailSender,
            IRealtimeNotifier realtimeNotifier,
            ILogger<NotificationDispatcher> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _realtimeNotifier = realtimeNotifier;
            _logger = logger;
        }

        public async Task DispatchAsync(
            WorkflowNotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            var roleNames = request.RoleNamesOrEmpty
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var directUserIds = request.UserIdsOrEmpty
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            var roleUserIds = Array.Empty<int>();
            if (roleNames.Length > 0)
            {
                roleUserIds = await _context.Users
                    .Where(u => u.IsActive && u.Role != null && roleNames.Contains(u.Role.Name.ToLower()))
                    .Select(u => u.Id)
                    .ToArrayAsync(cancellationToken);
            }

            var allUserIds = directUserIds
                .Concat(roleUserIds)
                .Distinct()
                .ToArray();

            var userRecipients = allUserIds.Length == 0
                ? []
                : await _context.Users
                    .Where(u => allUserIds.Contains(u.Id) && u.IsActive)
                    .Select(u => new NotificationUserRecipient(u.Id, u.Email))
                    .ToListAsync(cancellationToken);

            var clientRecipient = request.ClientId is null
                ? null
                : await _context.Clients
                    .Where(c => c.Id == request.ClientId.Value)
                    .Select(c => new NotificationClientRecipient(c.Id, c.Email))
                    .FirstOrDefaultAsync(cancellationToken);

            var clientPortalUserIds = request.ClientId is null
                ? Array.Empty<int>()
                : await _context.Users
                    .Where(u => u.ClientId == request.ClientId.Value && u.IsActive)
                    .Select(u => u.Id)
                    .ToArrayAsync(cancellationToken);

            var createdNotifications = new List<Notification>();
            foreach (var recipient in userRecipients)
            {
                var notification = new Notification(
                    recipient.UserId,
                    request.Type,
                    request.Title,
                    request.Body,
                    request.RelatedEntityId,
                    request.RelatedEntityType ?? string.Empty);

                createdNotifications.Add(notification);
                await _context.Notifications.AddAsync(notification, cancellationToken);
            }

            foreach (var clientUserId in clientPortalUserIds.Except(userRecipients.Select(x => x.UserId)))
            {
                var notification = new Notification(
                    clientUserId,
                    request.Type,
                    request.Title,
                    request.Body,
                    request.RelatedEntityId,
                    request.RelatedEntityType ?? string.Empty);

                createdNotifications.Add(notification);
                await _context.Notifications.AddAsync(notification, cancellationToken);
            }

            if (createdNotifications.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);

            if (request.PushToUsers)
            {
                foreach (var notification in createdNotifications)
                {
                    var unreadCount = await _context.Notifications.CountAsync(
                        n => n.UserId == notification.UserId && !n.IsRead,
                        cancellationToken);

                    var message = new NotificationRealtimeMessage(
                        notification.Id,
                        notification.UserId,
                        notification.Type,
                        notification.Title,
                        notification.Body,
                        notification.RelatedEntityId,
                        notification.RelatedEntityType,
                        notification.CreatedAt,
                        unreadCount);

                    await _realtimeNotifier.SendNotificationToUserAsync(
                        notification.UserId,
                        message,
                        cancellationToken);
                }
            }

            var workflowMessage = new WorkflowRealtimeMessage(
                request.EventName,
                request.Type,
                request.Title,
                request.Body,
                request.RelatedEntityId,
                request.RelatedEntityType,
                request.ClientId,
                DateTime.UtcNow);

            if (request.PushToUsers && allUserIds.Length > 0)
            {
                await _realtimeNotifier.SendWorkflowEventToUsersAsync(
                    allUserIds,
                    workflowMessage,
                    cancellationToken);
            }

            if (request.PushToUsers && roleNames.Length > 0)
            {
                await _realtimeNotifier.SendWorkflowEventToRolesAsync(
                    roleNames,
                    workflowMessage,
                    cancellationToken);
            }

            if (request.PushToClient && request.ClientId.HasValue)
            {
                await _realtimeNotifier.SendWorkflowEventToClientAsync(
                    request.ClientId.Value,
                    workflowMessage,
                    cancellationToken);
            }

            if (request.SendEmailToUsers)
            {
                foreach (var recipient in userRecipients.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                {
                    await SendEmailSafelyAsync(
                        recipient.Email!,
                        request.Title,
                        request.Body,
                        cancellationToken);
                }
            }

            if (request.SendEmailToClient && clientRecipient is not null && !string.IsNullOrWhiteSpace(clientRecipient.Email))
            {
                await SendEmailSafelyAsync(
                    clientRecipient.Email!,
                    request.Title,
                    request.Body,
                    cancellationToken);
            }
        }

        private async Task SendEmailSafelyAsync(
            string email,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            try
            {
                var htmlBody = $"""
                    <div style="font-family:Segoe UI,Arial,sans-serif;line-height:1.6">
                        <h2>{subject}</h2>
                        <p>{body}</p>
                        <p>This message was generated automatically by the Al-Nawras workflow system.</p>
                    </div>
                    """;

                await _emailSender.SendAsync(email, subject, htmlBody, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification email to {Email}.", email);
            }
        }

        private sealed record NotificationUserRecipient(int UserId, string? Email);
        private sealed record NotificationClientRecipient(Guid ClientId, string? Email);
    }
}
