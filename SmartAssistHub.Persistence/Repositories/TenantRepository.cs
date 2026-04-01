using Microsoft.EntityFrameworkCore;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;
using SmartAssistHub.Persistence.Context;

namespace SmartAssistHub.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = Slug.Normalize(slug);

        return await _context.Tenants
            .FirstOrDefaultAsync(
                t => t.Slug.Value == normalizedSlug,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        
        var normalizedSlug = Slug.Normalize(slug);

        return await _context.Tenants
            .AnyAsync(
                t => t.Slug.Value == normalizedSlug,
                cancellationToken);
    }

    public async Task<Slug> GenerateUniqueSlugAsync(
        string baseSlug,
        CancellationToken cancellationToken = default)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await SlugExistsAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return Slug.Generate(slug);
    }

    public async Task<PagedResult<Tenant>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Tenants
            .CountAsync(cancellationToken);

        var items = await _context.Tenants
            .OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Tenant>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task UpdateAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await Task.CompletedTask;
    }

    public async Task IncrementTokenUsageAsync(
        Guid tenantId,
        int tokensUsed,
        CancellationToken cancellationToken = default)
    {
        await _context.Tenants
            .Where(t => t.Id == tenantId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    t => t.TokensUsedThisMonth,
                    t => t.TokensUsedThisMonth + tokensUsed),
                cancellationToken);
    }
}