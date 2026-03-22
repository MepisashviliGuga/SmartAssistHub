using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record DocumentProcessingStartedEvent(
    Guid DocumentId,
    Guid TenantId) : DomainEvent;