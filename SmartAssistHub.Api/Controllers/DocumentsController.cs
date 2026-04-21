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
}