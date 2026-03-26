using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Conversations.Commands.UpdateConversationTitle;

public class UpdateConversationTitleCommandHandler
    : IRequestHandler<UpdateConversationTitleCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateConversationTitleCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        UpdateConversationTitleCommand command,
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
                "You can only rename your own conversations.");

        conversation.UpdateTitle(command.NewTitle);

        await _conversationRepository
            .UpdateAsync(conversation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}