namespace SmartAssistHub.Infrastructure.Configuration;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";

    public int MaxTokensPerChunk { get; set; } = 500;
    public int OverlapTokens { get; set; } = 50;
    public string EncodingName { get; set; } = "cl100k_base";
}