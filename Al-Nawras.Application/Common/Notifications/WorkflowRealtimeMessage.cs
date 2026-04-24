using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Application.Common.Notifications
{
    public sealed record WorkflowRealtimeMessage(
        string EventName,
        NotificationType Type,
        string Title,
        string Body,
        Guid? RelatedEntityId,
        string? RelatedEntityType,
        Guid? ClientId,
        DateTime OccurredAt
    );
}
