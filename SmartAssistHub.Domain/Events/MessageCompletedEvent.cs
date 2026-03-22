using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record MessageCompletedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid TenantId,
    int TokensUsed) : DomainEvent;