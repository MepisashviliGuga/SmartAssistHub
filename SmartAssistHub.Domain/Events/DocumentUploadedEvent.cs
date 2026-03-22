using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record DocumentUploadedEvent(
    Guid DocumentId,
    Guid TenantId,
    Guid UploadedByUserId,
    string FileName) : DomainEvent;