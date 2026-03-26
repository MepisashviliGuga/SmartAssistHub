using FluentValidation;

namespace SmartAssistHub.Application.Features.Users.Commands.TransferOwnership;

public class TransferOwnershipCommandValidator
    : AbstractValidator<TransferOwnershipCommand>
{
    public TransferOwnershipCommandValidator()
    {
        RuleFor(x => x.CurrentOwnerId)
            .NotEmpty()
            .WithMessage("CurrentOwnerId is required.");

        RuleFor(x => x.NewOwnerId)
            .NotEmpty()
            .WithMessage("NewOwnerId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x)
            .Must(x => x.CurrentOwnerId != x.NewOwnerId)
            .WithMessage("New owner must be different from current owner.");
    }
}