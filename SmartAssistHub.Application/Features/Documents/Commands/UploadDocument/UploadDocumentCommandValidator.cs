using FluentValidation;

namespace SmartAssistHub.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator
    : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required.");

        RuleFor(x => x.UploadedByUserId)
            .NotEmpty()
            .WithMessage("UploadedByUserId is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("FileName is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("ContentType is required.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File size must be greater than zero.");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required.");
    }
}