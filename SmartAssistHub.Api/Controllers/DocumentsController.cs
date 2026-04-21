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
    [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IDocumentChunker chunker,
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

    [HttpPost("test-search")]
    public async Task<IActionResult> TestSearch(
    [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IAiService ai,
    [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IDocumentSearchService search,
    CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid();

        var sampleChunks = new[]
        {
        "Our refund policy allows full refunds within 30 days of purchase. Simply contact customer support with your order number.",
        "We offer customer support 24/7 through email, chat, and phone. Average response time is 2 minutes for chat.",
        "Our company was founded in 2015 and has grown to over 500 employees across 15 countries worldwide.",
        "Premium subscribers get access to advanced AI features including custom model training and unlimited API calls.",
        "Shipping is free for orders over $50 within the United States. International shipping rates vary by destination."
    };

        foreach (var chunk in sampleChunks)
        {
            var embedding = await ai.GetEmbeddingAsync(chunk, cancellationToken);
            await search.IndexChunkAsync(
                chunkId: Guid.NewGuid(),
                documentId: Guid.NewGuid(),
                tenantId: tenantId,
                content: chunk,
                embedding: embedding,
                cancellationToken: cancellationToken);
        }

        var query = "How do I get my money back?";
        var results = await search.SearchRelevantChunksAsync(
            query, tenantId, maxChunks: 3, cancellationToken: cancellationToken);

        return Ok(new
        {
            tenantId,
            indexedChunks = sampleChunks.Length,
            query,
            topResults = results
        });
    }



    [HttpPost("test-full-pipeline")]
    public async Task<IActionResult> TestFullPipeline(
    IFormFile file,
    [FromServices] IMediator mediator,
    [FromServices] SmartAssistHub.Application.Common.Interfaces.Services.IDocumentSearchService searchService,
    CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tenantId = Guid.Parse("a068b85d-3242-4257-a266-56d3746053da");
        var userId = Guid.Parse("54D94DBC-BE6D-400E-B371-BED8317880DD");

        // Step 1 — upload document
        using var stream = file.OpenReadStream();
        var uploadResult = await mediator.Send(
            new SmartAssistHub.Application.Features.Documents
                .Commands.UploadDocument.UploadDocumentCommand(
                    TenantId: tenantId,
                    UploadedByUserId: userId,
                    FileName: file.FileName,
                    ContentType: file.ContentType,
                    FileSizeBytes: file.Length,
                    FileStream: stream),
            cancellationToken);

        // Step 2 — process the document
        await mediator.Send(
            new SmartAssistHub.Application.Features.Documents
                .Commands.ProcessDocument.ProcessDocumentCommand(
                    DocumentId: uploadResult.DocumentId,
                    TenantId: tenantId),
            cancellationToken);

        // Step 3 — search
        var query = "What is this document about?";
        var results = await searchService.SearchRelevantChunksAsync(
            query, tenantId, maxChunks: 3, cancellationToken);

        return Ok(new
        {
            documentId = uploadResult.DocumentId,
            query,
            topChunks = results
        });
    }
}