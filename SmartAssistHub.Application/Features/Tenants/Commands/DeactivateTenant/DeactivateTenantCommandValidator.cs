using FluentValidation;

namespace SmartAssistHub.Application.Features.Tenants.Commands.DeactivateTenant;

public class DeactivateTenantCommandValidator
    : AbstractValidator<DeactivateTenantCommand>
{
    public DeactivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");
    }
}