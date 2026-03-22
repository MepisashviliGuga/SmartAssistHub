using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record TenantCreatedEvent(
    Guid TenantId,
    string Name,
    string Slug) : DomainEvent;