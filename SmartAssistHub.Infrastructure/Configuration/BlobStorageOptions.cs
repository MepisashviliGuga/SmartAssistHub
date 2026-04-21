namespace SmartAssistHub.Infrastructure.Configuration;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = "documents";
}