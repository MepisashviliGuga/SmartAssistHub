using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SharpToken;
using SmartAssistHub.Application.Common.Interfaces.Services;
using SmartAssistHub.Infrastructure.Configuration;

namespace SmartAssistHub.Infrastructure.Ai;

public class OpenAiService : IAiService
{
    private readonly OpenAiOptions _options;
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;
    private readonly GptEncoding _encoding;

    public OpenAiService(IOptions<OpenAiOptions> options)
    {
        _options = options.Value;

        _chatClient = new ChatClient(
            model: _options.ChatModel,
            apiKey: _options.ApiKey);

        _embeddingClient = new EmbeddingClient(
            model: _options.EmbeddingModel,
            apiKey: _options.ApiKey);

        _encoding = GptEncoding.GetEncoding("cl100k_base");
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string systemPrompt,
        string userMessage,
        IReadOnlyList<string> contextChunks,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var stream = _chatClient.CompleteChatStreamingAsync(
            messages,
            cancellationToken: cancellationToken);

        await foreach (var update in stream)
        {
            foreach (var content in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(content.Text))
                    yield return content.Text;
            }
        }
    }

    public async Task<float[]> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var response = await _embeddingClient.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return response.Value.ToFloats().ToArray();
    }

    public Task<int> CountTokensAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var count = _encoding.Encode(text).Count;
        return Task.FromResult(count);
    }
}