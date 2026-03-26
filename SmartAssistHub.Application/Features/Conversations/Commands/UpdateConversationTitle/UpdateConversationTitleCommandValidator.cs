using FluentValidation;

namespace SmartAssistHub.Application.Features.Conversations.Commands.UpdateConversationTitle;

public class UpdateConversationTitleCommandValidator
    : AbstractValidator<UpdateConversationTitleCommand>
{
    public UpdateConversationTitleCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.NewTitle)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");
    }
}