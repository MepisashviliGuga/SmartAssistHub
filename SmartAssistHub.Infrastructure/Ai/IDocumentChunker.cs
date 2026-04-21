namespace SmartAssistHub.Infrastructure.Ai;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunkResult> Chunk(string text);
}

public record DocumentChunkResult(
    int Index,
    string Content,
    int TokenCount);