using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Infrastructure.Email;

public class EmailServiceStub : IEmailService
{
    public Task SendWelcomeEmailAsync(string toEmail, string recipientName,
        string workspaceUrl, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    public Task SendDocumentReadyEmailAsync(string toEmail, string recipientName,
        string documentName, int totalChunks, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    public Task SendDocumentFailedEmailAsync(string toEmail, string recipientName,
        string documentName, string reason, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}