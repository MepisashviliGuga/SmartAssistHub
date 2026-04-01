using Microsoft.EntityFrameworkCore;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Persistence.Context;

namespace SmartAssistHub.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(
                c => c.Id == id && c.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<PagedResult<Conversation>> GetPagedByUserAsync(
        Guid userId,
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Conversations
            .CountAsync(
                c => c.UserId == userId && c.TenantId == tenantId,
                cancellationToken);

        var items = await _context.Conversations
            .Where(c => c.UserId == userId && c.TenantId == tenantId)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Conversation>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
    }

    public async Task UpdateAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        _context.Conversations.Update(conversation);
        await Task.CompletedTask;
    }
}