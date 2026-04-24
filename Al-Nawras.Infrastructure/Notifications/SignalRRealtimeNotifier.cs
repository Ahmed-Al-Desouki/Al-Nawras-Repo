using Al_Nawras.Application.Common.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Al_Nawras.Infrastructure.Notifications
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<WorkflowHub> _hubContext;

        public SignalRRealtimeNotifier(IHubContext<WorkflowHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendNotificationToUserAsync(
            int userId,
            NotificationRealtimeMessage message,
            CancellationToken cancellationToken = default)
            => _hubContext.Clients
                .Group(WorkflowHubGroupNames.User(userId))
                .SendAsync(WorkflowHubEvents.NotificationReceived, message, cancellationToken);

        public Task SendWorkflowEventToUsersAsync(
            IEnumerable<int> userIds,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default)
            => SendToGroupsAsync(
                userIds.Select(WorkflowHubGroupNames.User),
                message,
                cancellationToken);

        public Task SendWorkflowEventToRolesAsync(
            IEnumerable<string> roleNames,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default)
            => SendToGroupsAsync(
                roleNames.Select(WorkflowHubGroupNames.Role),
                message,
                cancellationToken);

        public Task SendWorkflowEventToClientAsync(
            Guid clientId,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default)
            => _hubContext.Clients
                .Group(WorkflowHubGroupNames.Client(clientId))
                .SendAsync(message.EventName, message, cancellationToken);

        private Task SendToGroupsAsync(
            IEnumerable<string> groups,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken)
        {
            var targets = groups
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (targets.Length == 0)
                return Task.CompletedTask;

            return _hubContext.Clients
                .Groups(targets)
                .SendAsync(message.EventName, message, cancellationToken);
        }
    }
}
