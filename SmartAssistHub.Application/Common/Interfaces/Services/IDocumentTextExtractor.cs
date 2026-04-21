namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);
}