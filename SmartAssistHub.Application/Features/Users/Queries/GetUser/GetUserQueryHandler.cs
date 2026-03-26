using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Features.Users.Common;

namespace SmartAssistHub.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler
    : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(
        GetUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(user), query.UserId);

        if (user.TenantId != query.TenantId)
            throw new ForbiddenException(
                "User does not belong to this tenant.");

        return new UserDto(
            user.Id,
            user.TenantId,
            user.ExternalId,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            user.Role.ToString(),
            user.IsActive,
            user.IsEmailVerified,
            user.LastLoginAt,
            user.CreatedAt);
    }
}