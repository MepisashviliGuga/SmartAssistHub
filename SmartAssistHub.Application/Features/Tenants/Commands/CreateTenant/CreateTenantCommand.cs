using MediatR;
using SmartAssistHub.Domain.Enums;

namespace SmartAssistHub.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    TenantPlan Plan) : IRequest<CreateTenantResult>;

public record CreateTenantResult(
    Guid TenantId,
    string Name,
    string Slug);