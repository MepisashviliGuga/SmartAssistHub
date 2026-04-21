using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAssistHub.Application.Common.Interfaces.Services;
using SmartAssistHub.Infrastructure.Configuration;

namespace SmartAssistHub.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IOptions<BlobStorageOptions> options,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var settings = options.Value;

        var serviceClient = new BlobServiceClient(settings.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(
            settings.ContainerName);

        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var blobPath = BuildBlobPath(tenantId, fileName);
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var headers = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(
            fileStream,
            new BlobUploadOptions { HttpHeaders = headers },
            cancellationToken);

        _logger.LogInformation(
            "Uploaded blob {BlobPath} for tenant {TenantId}.",
            blobPath, tenantId);

        return blobPath;
    }

    public async Task<Stream> DownloadAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        var response = await blobClient.DownloadStreamingAsync(
            cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted blob {BlobPath}.", blobPath);
    }

    private static string BuildBlobPath(Guid tenantId, string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var sanitizedFileName = Path.GetFileName(fileName);
        return $"tenants/{tenantId}/documents/{timestamp}_{sanitizedFileName}";
    }
}