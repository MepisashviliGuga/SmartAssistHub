using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;

namespace SmartAssistHub.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public ConversationStatus Status { get; private set; }
    public int TotalTokensUsed { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation Create(
        Guid tenantId,
        Guid userId,
        string title)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "TenantId cannot be empty.", nameof(tenantId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "UserId cannot be empty.", nameof(userId));

        return new Conversation
        {
            TenantId = tenantId,
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(title)
                ? "New Conversation"
                : title.Trim(),
            Status = ConversationStatus.Active,
            TotalTokensUsed = 0
        };
    }

    public Message AddUserMessage(string content)
    {
        EnsureActive();
        EnsureNoActiveStream();

        var message = Message.Create(
            Id,
            TenantId,
            MessageRole.User,
            content,
            0,
            false);

        _messages.Add(message);
        SetUpdated();
        return message;
    }

    public Message AddAssistantMessage(string content, int tokensUsed)
    {
        EnsureActive();
        EnsureNoActiveStream();

        if (tokensUsed < 0)
            throw new ArgumentException(
                "Tokens used cannot be negative.", nameof(tokensUsed));

        var message = Message.Create(
            Id,
            TenantId,
            MessageRole.Assistant,
            content,
            tokensUsed,
            false);

        _messages.Add(message);
        TotalTokensUsed += tokensUsed;
        SetUpdated();
        return message;
    }

    public Message BeginStreamingResponse()
    {
        EnsureActive();
        EnsureNoActiveStream();

        var message = Message.Create(
            Id,
            TenantId,
            MessageRole.Assistant,
            string.Empty,
            0,
            true);

        _messages.Add(message);
        SetUpdated();
        return message;
    }

    public void CompleteStreamingResponse(
        Message message,
        string finalContent,
        int tokensUsed)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.ConversationId != Id)
            throw new InvalidOperationException(
                "Message does not belong to this conversation.");

        if (message.IsStreaming == false)
            throw new InvalidOperationException(
                "Cannot complete a message that is not currently streaming.");

        if (tokensUsed < 0)
            throw new ArgumentException(
                "Tokens used cannot be negative.", nameof(tokensUsed));

        message.UpdateContent(finalContent, tokensUsed);
        message.SetStreaming(false);
        TotalTokensUsed += tokensUsed;
        SetUpdated();
    }

    public void Archive()
    {
        Status = ConversationStatus.Archived;
        SetUpdated();
    }

    public void SoftDelete()
    {
        Status = ConversationStatus.Deleted;
        SetUpdated();
    }

    public void UpdateTitle(string newTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTitle);
        Title = newTitle.Trim();
        SetUpdated();
    }

    private void EnsureActive()
    {
        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException(
                "Cannot modify a conversation that is not active.");
    }

    private void EnsureNoActiveStream()
    {
        if (_messages.Any(m => m.IsStreaming))
            throw new InvalidOperationException(
                "Cannot add a new message while a streaming response is in progress.");
    }
}