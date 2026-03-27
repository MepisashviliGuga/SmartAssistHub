using System.Text.Json;
using SmartAssistHub.Application.Common.Interfaces;
using SmartAssistHub.Domain.Common;
using SmartAssistHub.Persistence.Context;
using SmartAssistHub.Persistence.Outbox;

namespace SmartAssistHub.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var outboxMessages = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().FullName!,
                Payload = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType()),
                OccurredAt = domainEvent.OccurredAt
            })
            .ToList();

        _context.OutboxMessages.AddRange(outboxMessages);

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());
    }
}