using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Users.Commands.TransferOwnership;

public class TransferOwnershipCommandHandler
    : IRequestHandler<TransferOwnershipCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferOwnershipCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        TransferOwnershipCommand command,
        CancellationToken cancellationToken)
    {
        var currentOwner = await _userRepository
            .GetByIdAsync(command.CurrentOwnerId, cancellationToken);

        if (currentOwner is null)
            throw new NotFoundException(
                nameof(currentOwner), command.CurrentOwnerId);

        if (currentOwner.TenantId != command.TenantId)
            throw new ForbiddenException(
                "Current owner does not belong to this tenant.");

        var newOwner = await _userRepository
            .GetByIdAsync(command.NewOwnerId, cancellationToken);

        if (newOwner is null)
            throw new NotFoundException(
                nameof(newOwner), command.NewOwnerId);

        if (newOwner.TenantId != command.TenantId)
            throw new ForbiddenException(
                "New owner does not belong to this tenant.");

        currentOwner.TransferOwnership(newOwner);

        await _userRepository.UpdateAsync(currentOwner, cancellationToken);
        await _userRepository.UpdateAsync(newOwner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}