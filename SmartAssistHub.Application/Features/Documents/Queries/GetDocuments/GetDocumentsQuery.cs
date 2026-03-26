using MediatR;
using SmartAssistHub.Application.Common.Models;
using SmartAssistHub.Application.Features.Documents.Common;

namespace SmartAssistHub.Application.Features.Documents.Queries.GetDocuments;

public record GetDocumentsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<DocumentDto>>;