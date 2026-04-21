using MediatR;

namespace SmartAssistHub.Application.Features.Documents.Commands.ProcessDocument;

public record ProcessDocumentCommand(
    Guid DocumentId,
    Guid TenantId
) : IRequest;