using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IBlobStorageService _blobStorage;

    public DocumentsController(IBlobStorageService blobStorage)
    {
        _blobStorage = blobStorage;
    }

    [HttpPost("test-upload")]
    public async Task<IActionResult> TestUpload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tenantId = Guid.NewGuid();

        using var stream = file.OpenReadStream();
        var blobPath = await _blobStorage.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            tenantId,
            cancellationToken);

        return Ok(new
        {
            tenantId,
            blobPath,
            fileName = file.FileName,
            size = file.Length
        });
    }

    [HttpPost("test-chunking")]
    public IActionResult TestChunking(
    [FromServices] SmartAssistHub.Infrastructure.Ai.IDocumentChunker chunker,
    [FromBody] TestChunkingRequest request)
    {
        var chunks = chunker.Chunk(request.Text);

        return Ok(new
        {
            totalChunks = chunks.Count,
            totalTokens = chunks.Sum(c => c.TokenCount),
            chunks = chunks.Select(c => new
            {
                c.Index,
                c.TokenCount,
                Preview = c.Content.Length > 100
                    ? c.Content[..100] + "..."
                    : c.Content,
                FullContent = c.Content
            })
        });
    }

    public record TestChunkingRequest(string Text);

    [HttpPost("test-ai")]
    public async Task<IActionResult> TestAi(
    [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IAiService ai,
    [FromBody] TestAiRequest request,
    CancellationToken cancellationToken)
    {
        var response = new System.Text.StringBuilder();

        await foreach (var token in ai.StreamCompletionAsync(
            systemPrompt: "You are a helpful assistant. Answer in one short sentence.",
            userMessage: request.Question,
            contextChunks: Array.Empty<string>(),
            cancellationToken: cancellationToken))
        {
            response.Append(token);
        }

        return Ok(new { question = request.Question, answer = response.ToString() });
    }

    [HttpPost("test-embedding")]
    public async Task<IActionResult> TestEmbedding(
        [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IAiService ai,
        [FromBody] TestEmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var embedding = await ai.GetEmbeddingAsync(request.Text, cancellationToken);

        return Ok(new
        {
            text = request.Text,
            dimensions = embedding.Length,
            firstTenValues = embedding.Take(10).ToArray()
        });
    }

    public record TestAiRequest(string Question);
    public record TestEmbeddingRequest(string Text);
}