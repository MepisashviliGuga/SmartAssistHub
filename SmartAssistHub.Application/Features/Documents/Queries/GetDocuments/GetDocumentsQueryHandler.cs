using MediatR;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Documents.Common;

namespace SmartAssistHub.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQueryHandler
    : IRequestHandler<GetDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsQueryHandler(
        IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<PagedResult<DocumentDto>> Handle(
        GetDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var pagedDocuments = await _documentRepository
            .GetPagedByTenantAsync(
                query.TenantId,
                query.Page,
                query.PageSize,
                cancellationToken);

        var dtos = pagedDocuments.Items
            .Select(d => new DocumentDto(
                d.Id,
                d.TenantId,
                d.UploadedByUserId,
                d.FileName.Value,
                d.ContentType,
                d.FileSizeBytes.ToString(),
                d.Status.ToString(),
                d.FailureReason,
                d.TotalChunkCount,
                d.EmbeddedChunkCount,
                d.ProcessedAt,
                d.CreatedAt))
            .ToList();

        return new PagedResult<DocumentDto>(
            dtos,
            pagedDocuments.TotalCount,
            pagedDocuments.Page,
            pagedDocuments.PageSize);
    }
}