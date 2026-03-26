using MediatR;

namespace SmartAssistHub.Application.Features.Conversations.Commands.CreateConversation;

public record CreateConversationCommand(
    Guid TenantId,
    Guid UserId,
    string Title) : IRequest<CreateConversationResult>;

public record CreateConversationResult(
    Guid ConversationId,
    string Title);