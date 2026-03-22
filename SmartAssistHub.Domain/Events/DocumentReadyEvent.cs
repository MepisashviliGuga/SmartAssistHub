using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record DocumentReadyEvent(
    Guid DocumentId,
    Guid TenantId,
    int TotalChunks) : DomainEvent;