using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Interfaces;

namespace SimpleInventoryManagementSystem.Application.Common;

public abstract class TransactionalHandlerBase<TRequest, TResponse>(
    ITransactionScopeFactory transactionScopeFactory,
    ISIMSDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILoggerFactory loggerFactory)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private ILogger Logger => loggerFactory.CreateLogger(GetType().Name);

    protected ISIMSDbContext DbContext { get; } = dbContext;
    protected IDateTimeProvider DateTimeProvider { get; } = dateTimeProvider;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var handlerName = GetType().Name;
        Logger.LogInformation("{Handler} handling {Request}", handlerName, typeof(TRequest).Name);

        await using var transaction = await transactionScopeFactory.BeginAsync(cancellationToken);
        try
        {
            var result = await HandleCoreAsync(request, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            Logger.LogInformation("{Handler} completed", handlerName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Handler} failed, rolling back", handlerName);
            await transaction.RollbackAsync(CancellationToken.None);
            await OnRollbackAsync(CancellationToken.None);
            throw;
        }
    }

    protected abstract Task<TResponse> HandleCoreAsync(TRequest request, CancellationToken cancellationToken);

    protected virtual Task OnRollbackAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected void WriteEvent(DomainEvent domainEvent)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
        DbContext.OutboxEvents.Add(OutboxEventEntity.Create(domainEvent.GetType().Name, payload, DateTimeProvider.UtcNow));
    }
}
