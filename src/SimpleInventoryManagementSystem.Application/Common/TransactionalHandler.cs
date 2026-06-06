using System.Text.Json;
using MediatR;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Interfaces;

namespace SimpleInventoryManagementSystem.Application.Common;

public abstract class TransactionalHandler<TRequest, TResponse>(
    ITransactionScopeFactory transactionScopeFactory,
    ISIMSDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected ISIMSDbContext DbContext { get; } = dbContext;
    protected IDateTimeProvider DateTimeProvider { get; } = dateTimeProvider;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await transactionScopeFactory.BeginAsync(cancellationToken);
        var result = await HandleCoreAsync(request, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    protected abstract Task<TResponse> HandleCoreAsync(TRequest request, CancellationToken cancellationToken);

    protected void WriteEvent(DomainEvent domainEvent)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
        DbContext.OutboxEvents.Add(OutboxEventEntity.Create(domainEvent.GetType().Name, payload, DateTimeProvider.UtcNow));
    }
}
