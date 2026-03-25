using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Features.Conversations.Commands.CreateConversation;

public class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, CreateConversationResult>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateConversationResult> Handle(
        CreateConversationCommand command,
        CancellationToken cancellationToken)
    {
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
                "Inactive users cannot create conversations.");

        var conversation = Conversation.Create(
            command.TenantId,
            command.UserId,
            command.Title);

        await _conversationRepository
            .AddAsync(conversation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateConversationResult(
            conversation.Id,
            conversation.Title);
    }
}