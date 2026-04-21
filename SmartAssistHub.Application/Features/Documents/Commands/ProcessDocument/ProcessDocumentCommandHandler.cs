using MediatR;
using Microsoft.Extensions.Logging;
using SmartAssistHub.Application.Common.Exceptions;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Application.Common.Interfaces.Repositories;
using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Application.Features.Documents.Commands.ProcessDocument;

public class ProcessDocumentCommandHandler
    : IRequestHandler<ProcessDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly IDocumentTextExtractor _textExtractor;
    private readonly IDocumentChunker _chunker;
    private readonly IAiService _aiService;
    private readonly IDocumentSearchService _searchService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDocumentCommandHandler> _logger;

    public ProcessDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorage,
        IDocumentTextExtractor textExtractor,
        IDocumentChunker chunker,
        IAiService aiService,
        IDocumentSearchService searchService,
        IUnitOfWork unitOfWork,
        ILogger<ProcessDocumentCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _blobStorage = blobStorage;
        _textExtractor = textExtractor;
        _chunker = chunker;
        _aiService = aiService;
        _searchService = searchService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
    ProcessDocumentCommand command,
    CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(
            command.DocumentId, command.TenantId, cancellationToken);

        if (document is null)
            throw new NotFoundException("Document", command.DocumentId);

        try
        {
            document.StartProcessing();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await using var fileStream = await _blobStorage.DownloadAsync(
                document.BlobStoragePath, cancellationToken);

            var fullText = await _textExtractor.ExtractTextAsync(
                fileStream, document.ContentType, cancellationToken);

            if (string.IsNullOrWhiteSpace(fullText))
            {
                document.MarkAsFailed("Document contains no extractable text.");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogWarning(
                    "Document {DocumentId} has no extractable text.",
                    command.DocumentId);
                return;
            }

            var chunks = _chunker.Chunk(fullText);

            if (chunks.Count == 0)
            {
                document.MarkAsFailed("Chunking produced no chunks.");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            foreach (var chunk in chunks)
            {
                var embedding = await _aiService.GetEmbeddingAsync(
                    chunk.Content, cancellationToken);

                var addedChunk = document.AddChunk(
                    content: chunk.Content,
                    chunkIndex: chunk.Index,
                    tokenCount: chunk.TokenCount);

                document.SetChunkEmbedding(chunk.Index, embedding);

                await _searchService.IndexChunkAsync(
                    chunkId: addedChunk.Id,
                    documentId: document.Id,
                    tenantId: document.TenantId,
                    content: chunk.Content,
                    embedding: embedding,
                    cancellationToken: cancellationToken);
            }

            document.MarkAsReady();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} processed successfully into {Count} chunks.",
                command.DocumentId, chunks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process document {DocumentId}.",
                command.DocumentId);

            try
            {
                document.MarkAsFailed("Processing failed. See logs for details.");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx,
                    "Failed to mark document {DocumentId} as failed.",
                    command.DocumentId);
            }

            throw;
        }
    }
}