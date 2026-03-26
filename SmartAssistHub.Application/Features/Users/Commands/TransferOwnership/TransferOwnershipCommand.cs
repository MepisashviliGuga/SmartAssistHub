using MediatR;

namespace SmartAssistHub.Application.Features.Users.Commands.TransferOwnership;

public record TransferOwnershipCommand(
    Guid CurrentOwnerId,
    Guid NewOwnerId,
    Guid TenantId) : IRequest<Unit>;