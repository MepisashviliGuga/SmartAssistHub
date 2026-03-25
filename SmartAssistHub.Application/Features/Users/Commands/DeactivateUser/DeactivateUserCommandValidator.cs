using FluentValidation;

namespace SmartAssistHub.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");
    }
}

