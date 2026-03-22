using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record UserRoleChangedEvent(
    Guid UserId,
    Guid TenantId,
    string PreviousRole,
    string NewRole) : DomainEvent;