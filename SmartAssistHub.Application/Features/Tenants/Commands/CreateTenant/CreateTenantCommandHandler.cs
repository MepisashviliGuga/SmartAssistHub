using MediatR;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler
    : IRequestHandler<CreateTenantCommand, CreateTenantResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTenantResult> Handle(
        CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slug.Generate(command.Name);

        var uniqueSlug = await _tenantRepository
            .GenerateUniqueSlugAsync(baseSlug.Value, cancellationToken);

        var tenant = Tenant.Create(command.Name, uniqueSlug, command.Plan);

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTenantResult(
            tenant.Id,
            tenant.Name,
            tenant.Slug.Value);
    }
}