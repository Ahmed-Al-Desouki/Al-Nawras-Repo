using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Common.Notifications
{
    public sealed record WorkflowNotificationRequest(
        NotificationType Type,
        string Title,
        string Body,
        Guid? RelatedEntityId = null,
        string? RelatedEntityType = null,
        IReadOnlyCollection<int>? UserIds = null,
        IReadOnlyCollection<string>? RoleNames = null,
        Guid? ClientId = null,
        bool SendEmailToUsers = true,
        bool SendEmailToClient = true,
        bool PushToUsers = true,
        bool PushToClient = true,
        string EventName = WorkflowHubEvents.WorkflowUpdated
    )
    {
        public IReadOnlyCollection<int> UserIdsOrEmpty => UserIds ?? Array.Empty<int>();
        public IReadOnlyCollection<string> RoleNamesOrEmpty => RoleNames ?? Array.Empty<string>();
    }
}
