using MediatR;

namespace SmartAssistHub.Application.Features.Conversations.Commands.ArchiveConversation;

public record ArchiveConversationCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId) : IRequest<Unit>;