using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record UserDeactivatedEvent(
    Guid UserId,
    Guid TenantId) : DomainEvent;