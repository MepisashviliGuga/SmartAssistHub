using MediatR;

namespace SmartAssistHub.Application.Features.Tenants.Commands.DeactivateTenant;

public record DeactivateTenantCommand(Guid TenantId) : IRequest<Unit>;