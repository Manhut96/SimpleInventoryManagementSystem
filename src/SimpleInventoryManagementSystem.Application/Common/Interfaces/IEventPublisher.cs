using SimpleInventoryManagementSystem.Domain.Events;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
