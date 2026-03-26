namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IDocumentSearchService
{
    Task<IReadOnlyList<string>> SearchRelevantChunksAsync(
        string query,
        Guid tenantId,
        int maxChunks = 5,
        CancellationToken cancellationToken = default);
}