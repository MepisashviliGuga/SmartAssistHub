using SmartAssistHub.Application.Common.Interfaces.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartAssistHub.Infrastructure.Documents;

public class PlainTextExtractor : IDocumentTextExtractor
{
    public async Task<string> ExtractTextAsync(
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        return contentType.ToLowerInvariant() switch
        {
            "application/pdf" => ExtractFromPdf(fileStream),
            var ct when ct.StartsWith("text/") => await ExtractFromTextAsync(
                fileStream, cancellationToken),
            _ => throw new NotSupportedException(
                $"Content type '{contentType}' is not yet supported.")
        };
    }

    private static string ExtractFromPdf(Stream fileStream)
    {
        using var document = PdfDocument.Open(fileStream);
        var text = new System.Text.StringBuilder();

        foreach (Page page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString().Trim();
    }

    private static async Task<string> ExtractFromTextAsync(
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(fileStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}