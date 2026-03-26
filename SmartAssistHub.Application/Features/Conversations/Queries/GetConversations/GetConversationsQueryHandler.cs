using MediatR;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Conversations.Common;

namespace SmartAssistHub.Application.Features.Conversations.Queries.GetConversations;

public class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, PagedResult<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationsQueryHandler(
        IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<PagedResult<ConversationDto>> Handle(
        GetConversationsQuery query,
        CancellationToken cancellationToken)
    {
        var pagedConversations = await _conversationRepository
            .GetPagedByUserAsync(
                query.UserId,
                query.TenantId,
                query.Page,
                query.PageSize,
                cancellationToken);

        var dtos = pagedConversations.Items
            .Select(c => new ConversationDto(
                c.Id,
                c.TenantId,
                c.UserId,
                c.Title,
                c.Status.ToString(),
                c.TotalTokensUsed,
                c.Messages.Count,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        return new PagedResult<ConversationDto>(
            dtos,
            pagedConversations.TotalCount,
            pagedConversations.Page,
            pagedConversations.PageSize);
    }
}