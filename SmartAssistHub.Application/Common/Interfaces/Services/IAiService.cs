namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IAiService
{
    IAsyncEnumerable<string> StreamCompletionAsync(
        string systemPrompt,
        string userMessage,
        IReadOnlyList<string> contextChunks,
        CancellationToken cancellationToken = default);

    Task<float[]> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task<int> CountTokensAsync(
        string text,
        CancellationToken cancellationToken = default);
}