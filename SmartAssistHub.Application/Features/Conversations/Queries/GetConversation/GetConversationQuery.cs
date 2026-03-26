using MediatR;
using SmartAssistHub.Application.Features.Conversations.Common;

namespace SmartAssistHub.Application.Features.Conversations.Queries.GetConversation;

public record GetConversationQuery(
    Guid ConversationId,
    Guid TenantId) : IRequest<ConversationDetailDto>;