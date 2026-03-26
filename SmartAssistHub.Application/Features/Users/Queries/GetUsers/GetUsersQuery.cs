using MediatR;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Users.Common;

namespace SmartAssistHub.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<UserDto>>;