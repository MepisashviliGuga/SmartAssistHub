using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
using SmartAssistHub.Domain.Events;

namespace SmartAssistHub.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Guid TenantId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = null!;
    public int TokensUsed { get; private set; }
    public bool IsStreaming { get; private set; }

    private Message() { }

    internal static Message Create(
        Guid conversationId,
        Guid tenantId,
        MessageRole role,
        string content,
        int tokensUsed,
        bool isStreaming)
    {
        if (isStreaming == false)
            ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (conversationId == Guid.Empty)
            throw new ArgumentException(
                "ConversationId cannot be empty.", nameof(conversationId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "TenantId cannot be empty.", nameof(tenantId));

        if (tokensUsed < 0)
            throw new ArgumentException(
                "Tokens used cannot be negative.", nameof(tokensUsed));

        return new Message
        {
            ConversationId = conversationId,
            TenantId = tenantId,
            Role = role,
            Content = content ?? string.Empty,
            TokensUsed = tokensUsed,
            IsStreaming = isStreaming
        };
    }

    internal void SetStreaming(bool isStreaming)
    {
        IsStreaming = isStreaming;
        SetUpdated();
    }

    internal void UpdateContent(string content, int tokensUsed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (tokensUsed < 0)
            throw new ArgumentException(
                "Tokens used cannot be negative.", nameof(tokensUsed));

        if (IsStreaming == false)
            throw new InvalidOperationException(
                "Cannot update content on a message that is not streaming.");

        Content = content;
        TokensUsed = tokensUsed;
        
        RaiseDomainEvent(new MessageCompletedEvent(
        Id,
        ConversationId,
        TenantId,
        tokensUsed));

        SetUpdated();
    }
}