using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;

namespace SmartAssistHub.Application.Features.Tenants.Commands.UpdateTenantPlan;

public class UpdateTenantPlanCommandHandler
    : IRequestHandler<UpdateTenantPlanCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTenantPlanCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        UpdateTenantPlanCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository
            .GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(
                nameof(tenant), command.TenantId);

        if (!tenant.IsActive)
            throw new ForbiddenException(
                "Cannot update plan of an inactive tenant.");

        tenant.UpdatePlan(command.NewPlan);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}