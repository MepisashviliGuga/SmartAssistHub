using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Events;

public record TenantPlanUpdatedEvent(
    Guid TenantId,
    string NewPlan) : DomainEvent;