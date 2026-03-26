using MediatR;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Users.Common;

namespace SmartAssistHub.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var pagedUsers = await _userRepository
            .GetPagedByTenantAsync(
                query.TenantId,
                query.Page,
                query.PageSize,
                cancellationToken);

        var dtos = pagedUsers.Items
            .Select(u => new UserDto(
                u.Id,
                u.TenantId,
                u.ExternalId,
                u.FirstName,
                u.LastName,
                u.Email.Value,
                u.Role.ToString(),
                u.IsActive,
                u.IsEmailVerified,
                u.LastLoginAt,
                u.CreatedAt))
            .ToList();

        return new PagedResult<UserDto>(
            dtos,
            pagedUsers.TotalCount,
            pagedUsers.Page,
            pagedUsers.PageSize);
    }
}