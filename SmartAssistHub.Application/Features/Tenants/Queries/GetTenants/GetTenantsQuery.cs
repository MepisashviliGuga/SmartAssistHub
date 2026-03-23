using MediatR;
using SmartAssistHub.Application.Features.Tenants.Common;

namespace SmartAssistHub.Application.Features.Tenants.Queries.GetTenants;

public record GetTenantsQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<GetTenantsResult>;

public record GetTenantsResult(
    IReadOnlyList<TenantDto> Tenants,
    int TotalCount,
    int Page,
    int PageSize);