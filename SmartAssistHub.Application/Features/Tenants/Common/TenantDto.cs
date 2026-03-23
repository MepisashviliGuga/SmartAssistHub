namespace SmartAssistHub.Application.Features.Tenants.Common;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    string Plan,
    int TokensUsedThisMonth,
    int MonthlyTokenLimit,
    DateTime CreatedAt);