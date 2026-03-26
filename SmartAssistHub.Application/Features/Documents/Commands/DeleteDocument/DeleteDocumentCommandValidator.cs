using FluentValidation;

namespace SmartAssistHub.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandValidator
    : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("DocumentId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");
    }
}