using Microsoft.Extensions.Options;
using SharpToken;
using SmartAssistHub.Application.Common.Interfaces.Services;
using SmartAssistHub.Infrastructure.Configuration;

namespace SmartAssistHub.Infrastructure.Ai;

public class RecursiveDocumentChunker : IDocumentChunker
{
    private readonly ChunkingOptions _options;
    private readonly GptEncoding _encoding;

    private static readonly string[] Separators =
    {
        "\n\n",      // paragraph
        "\n",        // line break
        ". ",        // sentence
        "? ",        // sentence
        "! ",        // sentence
        "; ",        // clause
        ", ",        // phrase
        " ",         // word
        ""           // character (last resort)
    };

    public RecursiveDocumentChunker(IOptions<ChunkingOptions> options)
    {
        _options = options.Value;
        _encoding = GptEncoding.GetEncoding(_options.EncodingName);
    }

    public IReadOnlyList<DocumentChunkResult> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<DocumentChunkResult>();

        var rawChunks = SplitRecursively(
            text,
            _options.MaxTokensPerChunk,
            separatorIndex: 0);

        var withOverlap = ApplyOverlap(
            rawChunks,
            _options.OverlapTokens);

        var result = new List<DocumentChunkResult>();
        for (int i = 0; i < withOverlap.Count; i++)
        {
            var content = withOverlap[i];
            var tokenCount = CountTokens(content);
            result.Add(new DocumentChunkResult(i, content, tokenCount));
        }

        return result;
    }

    private List<string> SplitRecursively(
        string text,
        int maxTokens,
        int separatorIndex)
    {
        if (CountTokens(text) <= maxTokens)
            return new List<string> { text };

        if (separatorIndex >= Separators.Length)
            return new List<string> { text };

        var separator = Separators[separatorIndex];
        var parts = separator.Length == 0
            ? SplitByCharacterCount(text, maxTokens)
            : text.Split(separator, StringSplitOptions.None).ToList();

        var result = new List<string>();
        var current = string.Empty;

        foreach (var part in parts)
        {
            var candidate = string.IsNullOrEmpty(current)
                ? part
                : current + separator + part;

            if (CountTokens(candidate) <= maxTokens)
            {
                current = candidate;
            }
            else
            {
                if (!string.IsNullOrEmpty(current))
                {
                    result.Add(current);
                    current = string.Empty;
                }

                if (CountTokens(part) > maxTokens)
                {
                    var subChunks = SplitRecursively(
                        part, maxTokens, separatorIndex + 1);
                    result.AddRange(subChunks);
                }
                else
                {
                    current = part;
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
            result.Add(current);

        return result;
    }

    private List<string> ApplyOverlap(
        List<string> chunks,
        int overlapTokens)
    {
        if (overlapTokens <= 0 || chunks.Count <= 1)
            return chunks;

        var result = new List<string> { chunks[0] };

        for (int i = 1; i < chunks.Count; i++)
        {
            var previous = chunks[i - 1];
            var current = chunks[i];

            var overlapText = GetLastTokens(previous, overlapTokens);
            result.Add(overlapText + " " + current);
        }

        return result;
    }

    private string GetLastTokens(string text, int tokenCount)
    {
        var tokens = _encoding.Encode(text);
        if (tokens.Count <= tokenCount)
            return text;

        var lastTokens = tokens
            .Skip(tokens.Count - tokenCount)
            .ToList();

        return _encoding.Decode(lastTokens);
    }

    private List<string> SplitByCharacterCount(string text, int maxTokens)
    {
        var approxChars = maxTokens * 4;
        var result = new List<string>();

        for (int i = 0; i < text.Length; i += approxChars)
        {
            var length = Math.Min(approxChars, text.Length - i);
            result.Add(text.Substring(i, length));
        }

        return result;
    }

    private int CountTokens(string text)
    {
        return _encoding.Encode(text).Count;
    }
}