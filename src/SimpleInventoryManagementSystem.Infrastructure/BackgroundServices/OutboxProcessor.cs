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
    private static readonly TimeSpan MinDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);

    private TimeSpan _pollDelay = MinDelay;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("OutboxProcessor starting");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var hadEvents = await ProcessPendingEventsAsync(stoppingToken);
            _pollDelay = hadEvents
                ? MinDelay
                : TimeSpan.FromSeconds(Math.Min(_pollDelay.TotalSeconds * 2, MaxDelay.TotalSeconds));
            await Task.Delay(_pollDelay, stoppingToken);
        }
    }

    private async Task<bool> ProcessPendingEventsAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<SIMSDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var unprocessed = await db.OutboxEvents
                .Where(e => e.ProcessedAt == null)
                .Take(100)
                .ToListAsync(stoppingToken);

            if (unprocessed.Count == 0)
                return false;

            logger.LogInformation("Processing {Count} outbox event(s)", unprocessed.Count);

            foreach (var outboxEvent in unprocessed)
            {
                var domainEvent = new OutboxDomainEvent(outboxEvent.EventType, outboxEvent.Payload, dateTimeProvider.UtcNow);
                await publisher.PublishAsync(domainEvent, stoppingToken);
                outboxEvent.MarkProcessed(dateTimeProvider.UtcNow);
            }

            await db.SaveChangesAsync(stoppingToken);
            logger.LogInformation("Processed {Count} outbox event(s) successfully", unprocessed.Count);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "OutboxProcessor encountered an error while processing events");
            return false;
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
