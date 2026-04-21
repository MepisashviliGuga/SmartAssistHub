using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Infrastructure.Search;

public class InMemoryDocumentSearchService : IDocumentSearchService
{
    private readonly IAiService _aiService;
    private readonly Dictionary<Guid, List<IndexedChunk>> _chunksByTenant = new();
    private readonly object _lock = new();

    public InMemoryDocumentSearchService(IAiService aiService)
    {
        _aiService = aiService;
    }

    public Task IndexChunkAsync(
        Guid chunkId,
        Guid documentId,
        Guid tenantId,
        string content,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        var chunk = new IndexedChunk(
            chunkId, documentId, tenantId, content, embedding);

        lock (_lock)
        {
            if (!_chunksByTenant.TryGetValue(tenantId, out var list))
            {
                list = new List<IndexedChunk>();
                _chunksByTenant[tenantId] = list;
            }
            list.Add(chunk);
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<string>> SearchRelevantChunksAsync(
        string query,
        Guid tenantId,
        int maxChunks = 5,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await _aiService.GetEmbeddingAsync(
            query, cancellationToken);

        List<IndexedChunk> tenantChunks;
        lock (_lock)
        {
            if (!_chunksByTenant.TryGetValue(tenantId, out var list))
                return Array.Empty<string>();

            tenantChunks = list.ToList();
        }

        return tenantChunks
            .Select(c => new
            {
                Content = c.Content,
                Similarity = CosineSimilarity(queryEmbedding, c.Embedding)
            })
            .OrderByDescending(c => c.Similarity)
            .Take(maxChunks)
            .Select(c => c.Content)
            .ToList();
    }

    public Task DeleteByDocumentAsync(
        Guid documentId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_chunksByTenant.TryGetValue(tenantId, out var list))
            {
                list.RemoveAll(c => c.DocumentId == documentId);
            }
        }

        return Task.CompletedTask;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length.");

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        magnitudeA = (float)Math.Sqrt(magnitudeA);
        magnitudeB = (float)Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0f;

        return dotProduct / (magnitudeA * magnitudeB);
    }

    private record IndexedChunk(
        Guid ChunkId,
        Guid DocumentId,
        Guid TenantId,
        string Content,
        float[] Embedding);
}