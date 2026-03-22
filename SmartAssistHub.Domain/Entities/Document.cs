using SmartAssistHub.Domain.Common;
using SmartAssistHub.Domain.Enums;
using SmartAssistHub.Domain.Events;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Domain.Entities;

public class Document : BaseEntity
{
    private const int MaxChunks = 10_000;

    private static readonly HashSet<string> AllowedContentTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    };

    public Guid TenantId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public FileName FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public FileSize FileSizeBytes { get; private set; } = null!;
    public string BlobStoragePath { get; private set; } = null!;
    public DocumentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private readonly List<DocumentChunk> _chunks = new();
    public IReadOnlyCollection<DocumentChunk> Chunks => _chunks.AsReadOnly();

    private Document() { }

    public static Document Create(
        Guid tenantId,
        Guid uploadedByUserId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string blobStoragePath)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "TenantId cannot be empty.", nameof(tenantId));

        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException(
                "UploadedByUserId cannot be empty.", nameof(uploadedByUserId));

        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobStoragePath);

        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException(
                $"Content type '{contentType}' is not supported.",
                nameof(contentType));

        ValidateBlobStoragePath(blobStoragePath);

        var document = new Document
        {
            TenantId = tenantId,
            UploadedByUserId = uploadedByUserId,
            FileName = FileName.Create(fileName),
            ContentType = contentType.ToLowerInvariant().Trim(),
            FileSizeBytes = FileSize.Create(fileSizeBytes),
            BlobStoragePath = blobStoragePath,
            Status = DocumentStatus.Uploaded,
            FailureReason = null,
            ProcessedAt = null
        };

        document.RaiseDomainEvent(new DocumentUploadedEvent(
            document.Id,
            document.TenantId,
            document.UploadedByUserId,
            document.FileName.Value));

        return document;
    }

    public void StartProcessing()
    {
        if (Status == DocumentStatus.Processing)
            throw new InvalidOperationException(
                "Document is already being processed.");

        if (Status == DocumentStatus.Ready)
            throw new InvalidOperationException(
                "Document has already been processed successfully.");

        if (Status != DocumentStatus.Uploaded &&
            Status != DocumentStatus.Failed)
            throw new InvalidOperationException(
                $"Cannot start processing from status '{Status}'.");

        Status = DocumentStatus.Processing;
        FailureReason = null;
        RaiseDomainEvent(new DocumentProcessingStartedEvent(Id, TenantId));
        SetUpdated();
    }

    public DocumentChunk AddChunk(
        string content,
        int chunkIndex,
        int tokenCount)
    {
        if (Status != DocumentStatus.Processing)
            throw new InvalidOperationException(
                "Chunks can only be added while document is processing.");

        if (chunkIndex < 0)
            throw new ArgumentException(
                "Chunk index cannot be negative.", nameof(chunkIndex));

        if (_chunks.Count >= MaxChunks)
            throw new InvalidOperationException(
                $"Document cannot exceed {MaxChunks} chunks.");

        if (_chunks.Any(c => c.ChunkIndex == chunkIndex))
            throw new InvalidOperationException(
                $"A chunk with index {chunkIndex} already exists.");

        if (tokenCount < 0)
            throw new ArgumentException(
                "Token count cannot be negative.", nameof(tokenCount));

        var chunk = DocumentChunk.Create(
            Id,
            TenantId,
            chunkIndex,
            content,
            tokenCount);

        _chunks.Add(chunk);
        SetUpdated();
        return chunk;
    }

    public void SetChunkEmbedding(int chunkIndex, float[] embedding)
    {
        if (Status != DocumentStatus.Processing)
            throw new InvalidOperationException(
                "Cannot set embeddings on a document that is not processing.");

        var chunk = _chunks.FirstOrDefault(c => c.ChunkIndex == chunkIndex)
            ?? throw new InvalidOperationException(
                $"Chunk with index {chunkIndex} not found.");

        chunk.SetEmbedding(embedding);
        SetUpdated();
    }

    public void MarkAsReady()
    {
        if (Status != DocumentStatus.Processing)
            throw new InvalidOperationException(
                "Only a processing document can be marked as ready.");

        if (!_chunks.Any())
            throw new InvalidOperationException(
                "Cannot mark a document as ready with no chunks.");

        if (_chunks.Any(c => !c.HasEmbedding))
            throw new InvalidOperationException(
                "Cannot mark a document as ready — some chunks are missing embeddings.");

        if (_chunks.Any(c => c.TenantId != TenantId))
            throw new InvalidOperationException(
                "Document contains chunks belonging to a different tenant.");

        _chunks.Sort((a, b) => a.ChunkIndex.CompareTo(b.ChunkIndex));

        Status = DocumentStatus.Ready;
        ProcessedAt = DateTime.UtcNow;
        FailureReason = null;
        RaiseDomainEvent(new DocumentReadyEvent(Id, TenantId, _chunks.Count));
        SetUpdated();
    }

    public void MarkAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status == DocumentStatus.Ready)
            throw new InvalidOperationException(
                "Cannot mark a successfully processed document as failed.");

        Status = DocumentStatus.Failed;
        FailureReason = reason;
        RaiseDomainEvent(new DocumentFailedEvent(Id, TenantId, reason));
        SetUpdated();
    }

    public bool IsAvailableForSearch =>
        Status == DocumentStatus.Ready;

    public int TotalChunkCount => _chunks.Count;

    public int EmbeddedChunkCount =>
        _chunks.Count(c => c.HasEmbedding);

    private static void ValidateBlobStoragePath(string path)
    {
        if (path.Contains(".."))
            throw new ArgumentException(
                "Blob path cannot contain '..'", nameof(path));

        if (path.StartsWith("/"))
            throw new ArgumentException(
                "Blob path cannot start with '/'", nameof(path));

        var invalidChars = new[] { '\\', '?', '#', '%' };
        if (invalidChars.Any(c => path.Contains(c)))
            throw new ArgumentException(
                "Blob path contains invalid characters.", nameof(path));
    }
}