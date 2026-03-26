using MediatR;

namespace SmartAssistHub.Application.Features.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(
    Guid DocumentId,
    Guid TenantId) : IRequest<Unit>;