using FluentValidation;

namespace SmartAssistHub.Application.Features.Tenants.Commands.UpdateTenantPlan;

public class UpdateTenantPlanCommandValidator
    : AbstractValidator<UpdateTenantPlanCommand>
{
    public UpdateTenantPlanCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.NewPlan)
            .IsInEnum()
            .WithMessage("Invalid plan specified.");
    }
}