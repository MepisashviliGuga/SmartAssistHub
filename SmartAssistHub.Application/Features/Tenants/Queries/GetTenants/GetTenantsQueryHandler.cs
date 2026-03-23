using MediatR;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Features.Tenants.Common;

namespace SmartAssistHub.Application.Features.Tenants.Queries.GetTenants;

public class GetTenantsQueryHandler
    : IRequestHandler<GetTenantsQuery, GetTenantsResult>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(
        ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<GetTenantsResult> Handle(
        GetTenantsQuery query,
        CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository
            .GetAllAsync(query.Page, query.PageSize, cancellationToken);

        var totalCount = await _tenantRepository
            .CountAsync(cancellationToken);

        var dtos = tenants
            .Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.Slug.Value,
                t.IsActive,
                t.Plan.ToString(),
                t.TokensUsedThisMonth,
                t.MonthlyTokenLimit,
                t.CreatedAt))
            .ToList();

        return new GetTenantsResult(
            dtos,
            totalCount,
            query.Page,
            query.PageSize);
    }
}