using SmartAssistHub.Domain.Common;

namespace SmartAssistHub.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public Guid TenantId { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = null!;
    public int TokenCount { get; private set; }

    private float[] _embedding = Array.Empty<float>();
    public IReadOnlyList<float> Embedding => _embedding;
    public bool HasEmbedding => _embedding.Length > 0;

    private DocumentChunk() { }

    internal static DocumentChunk Create(
        Guid documentId,
        Guid tenantId,
        int chunkIndex,
        string content,
        int tokenCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (documentId == Guid.Empty)
            throw new ArgumentException(
                "DocumentId cannot be empty.", nameof(documentId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "TenantId cannot be empty.", nameof(tenantId));

        if (chunkIndex < 0)
            throw new ArgumentException(
                "Chunk index cannot be negative.", nameof(chunkIndex));

        if (tokenCount < 0)
            throw new ArgumentException(
                "Token count cannot be negative.", nameof(tokenCount));

        return new DocumentChunk
        {
            DocumentId = documentId,
            TenantId = tenantId,
            ChunkIndex = chunkIndex,
            Content = content.Trim(),
            TokenCount = tokenCount,
            _embedding = Array.Empty<float>()
        };
    }

    internal void SetEmbedding(float[] embedding)
    {
        if (embedding == null || embedding.Length == 0)
            throw new ArgumentException(
                "Embedding cannot be empty.", nameof(embedding));

        _embedding = embedding.ToArray();
        SetUpdated();
    }

    public float[] GetEmbeddingSnapshot() => _embedding.ToArray();
}