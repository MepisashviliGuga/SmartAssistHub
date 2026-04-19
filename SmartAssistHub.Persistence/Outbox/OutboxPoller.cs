using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartAssistHub.Persistence.Context;

namespace SmartAssistHub.Persistence.Outbox;

public class OutboxPoller : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPoller> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public OutboxPoller(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPoller> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Poller started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessagesAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Poller stopped.");
    }

    private async Task ProcessOutboxMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var activity = new Activity("OutboxPoller.Process").Start();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var messages = await context.OutboxMessages
            .Where(m => m.PublishedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            activity?.SetTag("messages.count", 0);
            return;
        }

        activity?.SetTag("messages.count", messages.Count);

        _logger.LogInformation(
            "Processing {Count} outbox messages.", messages.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var message in messages)
        {
            try
            {
                await PublishAsync(message, cancellationToken);
                message.PublishedAt = DateTime.UtcNow;
                message.Error = null;
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish outbox message {MessageId} of type {Type}.",
                    message.Id, message.Type);

                message.Error = ex.Message;
                failureCount++;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
        }

        activity?.SetTag("messages.success", successCount);
        activity?.SetTag("messages.failed", failureCount);

        await context.SaveChangesAsync(cancellationToken);
    }

    private Task PublishAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        // Placeholder — real Service Bus publishing
        // implemented in Infrastructure layer
        _logger.LogDebug(
            "Publishing message {MessageId} of type {Type}.",
            message.Id, message.Type);

        return Task.CompletedTask;
    }
}