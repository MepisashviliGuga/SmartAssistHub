
namespace SmartAssistHub.Application.Common.Interfaces.Services;
public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunkResult> Chunk(string text);
}

public record DocumentChunkResult(
    int Index,
    string Content,
    int TokenCount);
