using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Features.Tenants.Common;

namespace SmartAssistHub.Application.Features.Tenants.Queries.GetTenant;

public class GetTenantQueryHandler
    : IRequestHandler<GetTenantQuery, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantQueryHandler(
        ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantDto> Handle(
        GetTenantQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(query.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), query.TenantId);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug.Value,
            tenant.IsActive,
            tenant.Plan.ToString(),
            tenant.TokensUsedThisMonth,
            tenant.MonthlyTokenLimit,
            tenant.CreatedAt);
    }
}