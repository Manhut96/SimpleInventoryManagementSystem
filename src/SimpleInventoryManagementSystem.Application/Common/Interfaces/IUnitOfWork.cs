using SimpleInventoryManagementSystem.Domain.Events;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface IUnitOfWork
{
    void Enqueue(DomainEvent domainEvent);
    Task CommitAsync(CancellationToken cancellationToken = default);
}
