using MediatR;
using SmartAssistHub.Application.Features.Users.Common;

namespace SmartAssistHub.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(
    Guid UserId,
    Guid TenantId) : IRequest<UserDto>;