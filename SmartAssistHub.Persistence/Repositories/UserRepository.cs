using Microsoft.EntityFrameworkCore;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;
using SmartAssistHub.Persistence.Context;

namespace SmartAssistHub.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
    string email,
    Guid tenantId,
    CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Email.FromDatabase(Email.Normalize(email));

        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.TenantId == tenantId &&
                     u.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task<User?> GetByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.ExternalId == externalId,
                cancellationToken);
    }

    public async Task<bool> EmailExistsInTenantAsync(
        string email,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Email.FromDatabase(Email.Normalize(email));

        return await _context.Users
            .AnyAsync(
                u => u.TenantId == tenantId &&
                     u.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task<PagedResult<User>> GetPagedByTenantAsync(
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Users
            .CountAsync(u => u.TenantId == tenantId, cancellationToken);

        var items = await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<User>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
}