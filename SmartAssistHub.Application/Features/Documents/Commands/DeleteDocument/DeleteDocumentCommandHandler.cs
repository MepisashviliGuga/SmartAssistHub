using MediatR;
using Microsoft.Extensions.Logging;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDocumentCommandHandler> _logger;

    public DeleteDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteDocumentCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        DeleteDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository
            .GetByIdAsync(
                command.DocumentId,
                command.TenantId,
                cancellationToken);

        if (document is null)
            throw new NotFoundException(
                nameof(document), command.DocumentId);

        var blobPath = document.BlobStoragePath;

        await _documentRepository
            .DeleteAsync(document, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await TryDeleteBlobAsync(blobPath, cancellationToken);

        return Unit.Value;
    }

    private async Task TryDeleteBlobAsync(
        string blobPath,
        CancellationToken cancellationToken)
    {
        try
        {
            await _blobStorageService
                .DeleteAsync(blobPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete blob {BlobPath} " +
                "after document deletion. " +
                "Cleanup job will handle this.",
                blobPath);
        }
    }
}