using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler
    : IRequestHandler<ChangeUserRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserRoleCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        ChangeUserRoleCommand command,
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

        user.ChangeRole(command.NewRole);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}