namespace Al_Nawras.Application.Common.Notifications
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(
            WorkflowNotificationRequest request,
            CancellationToken cancellationToken = default);
    }
}
