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
}