using MediatR;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Tenants.Common;

namespace SmartAssistHub.Application.Features.Tenants.Queries.GetTenants;

public class GetTenantsQueryHandler
    : IRequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(
        ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<PagedResult<TenantDto>> Handle(
        GetTenantsQuery query,
        CancellationToken cancellationToken)
    {
        var pagedTenants = await _tenantRepository
            .GetPagedAsync(query.Page, query.PageSize, cancellationToken);

        var dtos = pagedTenants.Items
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

        return new PagedResult<TenantDto>(
            dtos,
            pagedTenants.TotalCount,
            pagedTenants.Page,
            pagedTenants.PageSize);
    }
}