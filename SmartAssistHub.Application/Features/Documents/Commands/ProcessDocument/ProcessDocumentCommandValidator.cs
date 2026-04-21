using FluentValidation;

namespace SmartAssistHub.Application.Features.Documents.Commands.ProcessDocument;

public class ProcessDocumentCommandValidator
    : AbstractValidator<ProcessDocumentCommand>
{
    public ProcessDocumentCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotEmpty();
        RuleFor(c => c.TenantId).NotEmpty();
    }
}