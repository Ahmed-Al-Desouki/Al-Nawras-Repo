namespace Al_Nawras.Application.Common.Notifications
{
    public interface IRealtimeNotifier
    {
        Task SendNotificationToUserAsync(
            int userId,
            NotificationRealtimeMessage message,
            CancellationToken cancellationToken = default);

        Task SendWorkflowEventToUsersAsync(
            IEnumerable<int> userIds,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default);

        Task SendWorkflowEventToRolesAsync(
            IEnumerable<string> roleNames,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default);

        Task SendWorkflowEventToClientAsync(
            Guid clientId,
            WorkflowRealtimeMessage message,
            CancellationToken cancellationToken = default);
    }
}
