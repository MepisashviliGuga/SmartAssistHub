using MediatR;
using SmartAssistHub.Application.Features.Documents.Common;

namespace SmartAssistHub.Application.Features.Documents.Queries.GetDocument;

public record GetDocumentQuery(
    Guid DocumentId,
    Guid TenantId) : IRequest<DocumentDto>;