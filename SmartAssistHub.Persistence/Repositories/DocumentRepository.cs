using Microsoft.EntityFrameworkCore;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Persistence.Context;

namespace SmartAssistHub.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(
                d => d.Id == id && d.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<PagedResult<Document>> GetPagedByTenantAsync(
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Documents
            .CountAsync(d => d.TenantId == tenantId, cancellationToken);

        var items = await _context.Documents
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Document>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }

    public async Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        _context.Documents.Remove(document);
        await Task.CompletedTask;
    }
}