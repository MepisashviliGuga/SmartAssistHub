using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartAssistHub.Application.Common.Interfaces.Services;
using SmartAssistHub.Infrastructure.Ai;
using SmartAssistHub.Infrastructure.Configuration;
using SmartAssistHub.Infrastructure.Email;
using SmartAssistHub.Infrastructure.Search;
using SmartAssistHub.Infrastructure.Storage;

namespace SmartAssistHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BlobStorageOptions>(
            configuration.GetSection(BlobStorageOptions.SectionName));

        services.Configure<OpenAiOptions>(
            configuration.GetSection(OpenAiOptions.SectionName));

        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
        services.AddSingleton<IAiService, OpenAiService>();
        services.AddScoped<IDocumentSearchService, DocumentSearchServiceStub>();
        services.AddScoped<IEmailService, ConsoleEmailService>();

        services.Configure<ChunkingOptions>(
            configuration.GetSection(ChunkingOptions.SectionName));

        services.AddSingleton<IDocumentChunker, RecursiveDocumentChunker>();
        return services;
    }
}