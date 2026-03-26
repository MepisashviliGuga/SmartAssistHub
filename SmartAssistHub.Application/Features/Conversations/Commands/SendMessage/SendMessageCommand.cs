using MediatR;

namespace SmartAssistHub.Application.Features.Conversations.Commands.SendMessage;

public record SendMessageCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId,
    string Content,
    Func<string, Task>? OnTokenReceived = null) : IRequest<SendMessageResult>;

public record SendMessageResult(
    Guid MessageId,
    int TokensUsed);