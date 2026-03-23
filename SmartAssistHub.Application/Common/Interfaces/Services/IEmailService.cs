namespace SmartAssistHub.Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string toEmail,
        string recipientName,
        string workspaceUrl,
        CancellationToken cancellationToken = default);

    Task SendDocumentReadyEmailAsync(
        string toEmail,
        string recipientName,
        string documentName,
        int totalChunks,
        CancellationToken cancellationToken = default);

    Task SendDocumentFailedEmailAsync(
        string toEmail,
        string recipientName,
        string documentName,
        string reason,
        CancellationToken cancellationToken = default);
}