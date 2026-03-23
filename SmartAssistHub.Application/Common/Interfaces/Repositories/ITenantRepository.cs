using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;
namespace SmartAssistHub.Application.Common.Interfaces.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Tenant?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
    string slug,
    CancellationToken cancellationToken = default);

    Task<Slug> GenerateUniqueSlugAsync(
        string baseSlug,
        CancellationToken cancellationToken = default);
}