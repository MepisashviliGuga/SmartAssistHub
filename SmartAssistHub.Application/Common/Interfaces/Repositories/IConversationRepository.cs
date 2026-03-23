using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Common.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> GetByUserIdAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);
}