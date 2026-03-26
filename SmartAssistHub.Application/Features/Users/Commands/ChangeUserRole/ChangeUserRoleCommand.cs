using MediatR;
using SmartAssistHub.Domain.Enums;

namespace SmartAssistHub.Application.Features.Users.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(
    Guid UserId,
    Guid TenantId,
    UserRole NewRole) : IRequest<Unit>;