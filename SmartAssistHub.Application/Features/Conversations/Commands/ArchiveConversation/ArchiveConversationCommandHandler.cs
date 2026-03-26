using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Conversations.Commands.ArchiveConversation;

public class ArchiveConversationCommandHandler
    : IRequestHandler<ArchiveConversationCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        ArchiveConversationCommand command,
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

        if (conversation.UserId != command.UserId)
            throw new ForbiddenException(
                "You can only archive your own conversations.");

        conversation.Archive();

        await _conversationRepository
            .UpdateAsync(conversation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}