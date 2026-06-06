using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Infrastructure.Persistence;

namespace SimpleInventoryManagementSystem.Infrastructure.BackgroundServices;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingEventsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<SIMSDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var unprocessed = await db.OutboxEvents
                .Where(e => e.ProcessedAt == null)
                .ToListAsync(stoppingToken);

            foreach (var outboxEvent in unprocessed)
            {
                var domainEvent = new OutboxDomainEvent(outboxEvent.EventType, outboxEvent.Payload, dateTimeProvider.UtcNow);
                await publisher.PublishAsync(domainEvent, stoppingToken);
                outboxEvent.MarkProcessed(dateTimeProvider.UtcNow);
            }

            await db.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "OutboxProcessor encountered an error while processing events");
        }
    }
}

/// <summary>
/// Thin wrapper that carries outbox event data through the IEventPublisher pipeline.
/// </summary>
internal sealed record OutboxDomainEvent(
    string EventType,
    string Payload,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
