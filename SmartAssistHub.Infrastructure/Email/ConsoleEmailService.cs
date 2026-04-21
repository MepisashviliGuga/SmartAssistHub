using Microsoft.Extensions.Logging;
using SmartAssistHub.Application.Common.Interfaces.Services;

namespace SmartAssistHub.Infrastructure.Email;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendWelcomeEmailAsync(
        string toEmail,
        string recipientName,
        string workspaceUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Welcome email to {Email} ({Name}) — workspace: {Url}",
            toEmail, recipientName, workspaceUrl);

        return Task.CompletedTask;
    }

    public Task SendDocumentReadyEmailAsync(
        string toEmail,
        string recipientName,
        string documentName,
        int totalChunks,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Document ready email to {Email} ({Name}) — " +
            "document: {DocumentName}, chunks: {Chunks}",
            toEmail, recipientName, documentName, totalChunks);

        return Task.CompletedTask;
    }

    public Task SendDocumentFailedEmailAsync(
        string toEmail,
        string recipientName,
        string documentName,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Document failed email to {Email} ({Name}) — " +
            "document: {DocumentName}, reason: {Reason}",
            toEmail, recipientName, documentName, reason);

        return Task.CompletedTask;
    }
}