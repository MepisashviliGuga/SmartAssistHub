using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Tenants.Commands.DeactivateTenant;

public class DeactivateTenantCommandHandler
    : IRequestHandler<DeactivateTenantCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        DeactivateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), command.TenantId);

        tenant.Deactivate();

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}