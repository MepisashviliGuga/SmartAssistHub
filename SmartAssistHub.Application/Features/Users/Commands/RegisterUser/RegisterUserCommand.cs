using MediatR;
using SmartAssistHub.Domain.Enums;

namespace SmartAssistHub.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    Guid TenantId,
    string ExternalId,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role = UserRole.Member) : IRequest<RegisterUserResult>;

public record RegisterUserResult(
    Guid UserId,
    string FullName,
    string Email);