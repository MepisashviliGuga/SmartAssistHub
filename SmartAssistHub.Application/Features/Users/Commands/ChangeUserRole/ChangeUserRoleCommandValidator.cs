using FluentValidation;

namespace SmartAssistHub.Application.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator
    : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage("Invalid role specified.");
    }
}