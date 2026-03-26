using MediatR;

namespace SmartAssistHub.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentCommand(
    Guid TenantId,
    Guid UploadedByUserId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream) : IRequest<UploadDocumentResult>;

public record UploadDocumentResult(
    Guid DocumentId,
    string FileName,
    string Status);