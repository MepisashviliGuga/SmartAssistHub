using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Infrastructure.Search;

public class DocumentSearchServiceStub : IDocumentSearchService
{
    public Task<IReadOnlyList<string>> SearchRelevantChunksAsync(
        string query, Guid tenantId, int maxChunks = 5,
        CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}