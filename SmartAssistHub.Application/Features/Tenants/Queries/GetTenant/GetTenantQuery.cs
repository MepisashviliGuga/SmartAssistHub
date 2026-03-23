using MediatR;
using SmartAssistHub.Application.Features.Tenants.Common;

namespace SmartAssistHub.Application.Features.Tenants.Queries.GetTenant;

public record GetTenantQuery(Guid TenantId) : IRequest<TenantDto>;