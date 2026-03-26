using MediatR;

namespace SmartAssistHub.Application.Features.Conversations.Commands.UpdateConversationTitle;

public record UpdateConversationTitleCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId,
    string NewTitle) : IRequest<Unit>;