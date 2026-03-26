using MediatR;

namespace SmartAssistHub.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId, Guid TenantId) : IRequest<Unit>;

