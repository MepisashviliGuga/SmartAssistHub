using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Common.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Conversation>> GetPagedByUserAsync(
        Guid userId,
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}