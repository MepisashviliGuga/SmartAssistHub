using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record DocumentFailedEvent(
    Guid DocumentId,
    Guid TenantId,
    string Reason) : DomainEvent;