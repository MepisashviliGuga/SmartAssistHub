using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record UserEmailVerifiedEvent(
    Guid UserId,
    Guid TenantId,
    string Email) : DomainEvent;