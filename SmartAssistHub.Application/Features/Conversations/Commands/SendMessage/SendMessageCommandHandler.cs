using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Application.Features.Conversations.Commands.SendMessage;

public class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAiService _aiService;
    private readonly IDocumentSearchService _documentSearchService;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IAiService aiService,
        IDocumentSearchService documentSearchService,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _aiService = aiService;
        _documentSearchService = documentSearchService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SendMessageResult> Handle(
        SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository
            .GetByIdAsync(
                command.ConversationId,
                command.TenantId,
                cancellationToken);

        if (conversation is null)
            throw new NotFoundException(
                nameof(conversation), command.ConversationId);

        var user = await _userRepository
            .GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(
                nameof(user), command.UserId);

        if (user.TenantId != command.TenantId)
            throw new ForbiddenException(
                "User does not belong to this tenant.");

        if (!user.IsActive)
            throw new ForbiddenException(
                "Inactive users cannot send messages.");

        var tenant = await _tenantRepository
            .GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), command.TenantId);

        if (!tenant.HasTokenBudgetRemaining())
            throw new ForbiddenException(
                "Monthly token budget has been exceeded.");

        conversation.AddUserMessage(command.Content);

        var contextChunks = await _documentSearchService
            .SearchRelevantChunksAsync(
                command.Content,
                command.TenantId,
                maxChunks: 5,
                cancellationToken);

        var systemPrompt = BuildSystemPrompt(contextChunks);

        var streamingMessage = conversation.BeginStreamingResponse();

        var fullResponse = new System.Text.StringBuilder();
        var tokenCount = 0;

        await foreach (var token in _aiService.StreamCompletionAsync(
            systemPrompt,
            command.Content,
            contextChunks,
            cancellationToken))
        {
            fullResponse.Append(token);

            if (command.OnTokenReceived is not null)
                await command.OnTokenReceived(token);
        }

        tokenCount = await _aiService.CountTokensAsync(
            fullResponse.ToString(),
            cancellationToken);

        conversation.CompleteStreamingResponse(
            streamingMessage,
            fullResponse.ToString(),
            tokenCount);

        tenant.RecordTokenUsage(tokenCount);

        await _conversationRepository
            .UpdateAsync(conversation, cancellationToken);

        await _tenantRepository
            .UpdateAsync(tenant, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendMessageResult(
            streamingMessage.Id,
            tokenCount);
    }

    private static string BuildSystemPrompt(
        IReadOnlyList<string> contextChunks)
    {
        if (!contextChunks.Any())
            return "You are a helpful assistant. " +
                   "Answer the user's question as helpfully as possible.";

        var context = string.Join("\n\n", contextChunks);

        return $"""
                You are a helpful assistant with access to company documents.
                Answer the user's question based ONLY on the following context.
                If the answer cannot be found in the context, say so clearly.
                Do not make up information.

                Context:
                {context}
                """;
    }
}