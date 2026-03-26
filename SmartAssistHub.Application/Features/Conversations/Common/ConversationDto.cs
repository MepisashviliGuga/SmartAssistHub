namespace SmartAssistHub.Application.Features.Conversations.Common;

public record ConversationDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string Title,
    string Status,
    int TotalTokensUsed,
    int MessageCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record MessageDto(
    Guid Id,
    string Role,
    string Content,
    int TokensUsed,
    bool IsStreaming,
    DateTime CreatedAt);

public record ConversationDetailDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string Title,
    string Status,
    int TotalTokensUsed,
    DateTime CreatedAt,
    IReadOnlyList<MessageDto> Messages);