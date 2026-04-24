using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Common.Notifications
{
    public sealed record NotificationRealtimeMessage(
        Guid NotificationId,
        int UserId,
        NotificationType Type,
        string Title,
        string Body,
        Guid? RelatedEntityId,
        string? RelatedEntityType,
        DateTime CreatedAt,
        int UnreadCount
    );
}
