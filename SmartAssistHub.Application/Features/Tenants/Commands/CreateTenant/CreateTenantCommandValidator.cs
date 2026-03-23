using FluentValidation;

namespace SmartAssistHub.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator
    : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required.")
            .MaximumLength(100)
            .WithMessage("Tenant name cannot exceed 100 characters.")
            .MinimumLength(2)
            .WithMessage("Tenant name must be at least 2 characters.");

        RuleFor(x => x.Plan)
            .IsInEnum()
            .WithMessage("Invalid tenant plan specified.");
    }
}