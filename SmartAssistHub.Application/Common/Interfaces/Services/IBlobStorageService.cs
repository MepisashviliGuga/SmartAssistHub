namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string blobPath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default);
}