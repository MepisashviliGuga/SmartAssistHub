using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Domain.Entities;

namespace SmartAssistHub.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository
            .GetByExternalIdAsync(
                command.ExternalId,
                cancellationToken);

        if (existingUser is not null)
            return new RegisterUserResult(
                existingUser.Id,
                existingUser.FullName,
                existingUser.Email.Value);

        var tenant = await _tenantRepository
            .GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), command.TenantId);

        if (!tenant.IsActive)
            throw new ForbiddenException(
                "Cannot register user for an inactive tenant.");

        var user = User.Create(
            command.TenantId,
            command.ExternalId,
            command.FirstName,
            command.LastName,
            command.Email,
            command.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(
            user.Id,
            user.FullName,
            user.Email.Value);
    }
}