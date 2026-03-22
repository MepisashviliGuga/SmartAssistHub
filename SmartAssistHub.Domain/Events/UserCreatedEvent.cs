using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record UserCreatedEvent(
    Guid UserId,
    Guid TenantId,
    string Email,
    string Role) : DomainEvent;