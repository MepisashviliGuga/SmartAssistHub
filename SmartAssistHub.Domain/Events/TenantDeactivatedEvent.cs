using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record TenantDeactivatedEvent(
    Guid TenantId) : DomainEvent;