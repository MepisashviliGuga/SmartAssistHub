using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsInTenantAsync(
        string email,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default);
}