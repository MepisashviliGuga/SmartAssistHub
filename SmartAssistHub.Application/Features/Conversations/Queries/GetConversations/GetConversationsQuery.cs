using MediatR;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Conversations.Common;

namespace SmartAssistHub.Application.Features.Conversations.Queries.GetConversations;

public record GetConversationsQuery(
    Guid TenantId,
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ConversationDto>>;