namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IDocumentSearchService
{
    Task IndexChunkAsync(
        Guid chunkId,
        Guid documentId,
        Guid tenantId,
        string content,
        float[] embedding,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SearchRelevantChunksAsync(
        string query,
        Guid tenantId,
        int maxChunks = 5,
        CancellationToken cancellationToken = default);

    Task DeleteByDocumentAsync(
        Guid documentId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}