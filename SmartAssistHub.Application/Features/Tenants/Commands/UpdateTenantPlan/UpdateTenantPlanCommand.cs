using MediatR;
using SmartAssistHub.Domain.Enums;

namespace SmartAssistHub.Application.Features.Tenants.Commands.UpdateTenantPlan;

public record UpdateTenantPlanCommand(
    Guid TenantId,
    TenantPlan NewPlan) : IRequest<Unit>;