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
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SIMSDbContext>();
        var outboxEventRepository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var pendingIds = await outboxEventRepository.GetPendingIdsAsync(100, stoppingToken);
        if (pendingIds.Count == 0) return false;

        var processedCount = 0;
        foreach (var eventId in pendingIds)
        {
            var wasProcessed = await ProcessSingleEventAsync(
                eventId, dbContext, outboxEventRepository, publisher, dateTimeProvider, stoppingToken);
            if (wasProcessed) processedCount++;
        }

        logger.LogInformation("Processed {Count} outbox event(s)", processedCount);
        return processedCount > 0;
    }

    private async Task<bool> ProcessSingleEventAsync(
        Guid eventId,
        SIMSDbContext dbContext,
        IOutboxEventRepository outboxEventRepository,
        IEventPublisher publisher,
        IDateTimeProvider dateTimeProvider,
        CancellationToken stoppingToken)
    {
        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            var outboxEvent = await outboxEventRepository.TryLockSingleAsync(eventId, stoppingToken);
            if (outboxEvent is null) return false;

            var domainEvent = new OutboxDomainEvent(outboxEvent.EventType, outboxEvent.Payload, dateTimeProvider.UtcNow);
            await publisher.PublishAsync(domainEvent, stoppingToken);
            outboxEvent.MarkProcessed(dateTimeProvider.UtcNow);
            await dbContext.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "OutboxProcessor failed to process outbox event {EventId}", eventId);
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
