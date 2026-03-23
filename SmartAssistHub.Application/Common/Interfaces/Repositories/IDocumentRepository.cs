using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Common.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Document document,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default);
}