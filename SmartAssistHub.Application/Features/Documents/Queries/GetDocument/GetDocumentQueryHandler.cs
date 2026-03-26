using MediatR;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Features.Documents.Common;

namespace SmartAssistHub.Application.Features.Documents.Queries.GetDocument;

public class GetDocumentQueryHandler
    : IRequestHandler<GetDocumentQuery, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentQueryHandler(
        IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentDto> Handle(
        GetDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository
            .GetByIdAsync(
                query.DocumentId,
                query.TenantId,
                cancellationToken);

        if (document is null)
            throw new NotFoundException(
                nameof(document), query.DocumentId);

        return new DocumentDto(
            document.Id,
            document.TenantId,
            document.UploadedByUserId,
            document.FileName.Value,
            document.ContentType,
            document.FileSizeBytes.ToString(),
            document.Status.ToString(),
            document.FailureReason,
            document.TotalChunkCount,
            document.EmbeddedChunkCount,
            document.ProcessedAt,
            document.CreatedAt);
    }
}