namespace SmartAssistHub.Application.Features.Users.Common;

public record UserDto(
    Guid Id,
    Guid TenantId,
    string ExternalId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    DateTime? LastLoginAt,
    DateTime CreatedAt);