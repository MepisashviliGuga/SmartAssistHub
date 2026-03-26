using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Features.Conversations.Common;

namespace SmartAssistHub.Application.Features.Conversations.Queries.GetConversation;

public class GetConversationQueryHandler
    : IRequestHandler<GetConversationQuery, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationQueryHandler(
        IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDetailDto> Handle(
        GetConversationQuery query,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository
            .GetByIdAsync(
                query.ConversationId,
                query.TenantId,
                cancellationToken);

        if (conversation is null)
            throw new NotFoundException(
                nameof(conversation), query.ConversationId);

        var messages = conversation.Messages
            .Select(m => new MessageDto(
                m.Id,
                m.Role.ToString(),
                m.Content,
                m.TokensUsed,
                m.IsStreaming,
                m.CreatedAt))
            .ToList();

        return new ConversationDetailDto(
            conversation.Id,
            conversation.TenantId,
            conversation.UserId,
            conversation.Title,
            conversation.Status.ToString(),
            conversation.TotalTokensUsed,
            conversation.CreatedAt,
            messages);
    }
}