using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Infrastructure.Ai;

public class AiServiceStub : IAiService
{
    public IAsyncEnumerable<string> StreamCompletionAsync(
        string systemPrompt, string userMessage,
        IReadOnlyList<string> contextChunks, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    public Task<int> CountTokensAsync(string text, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}