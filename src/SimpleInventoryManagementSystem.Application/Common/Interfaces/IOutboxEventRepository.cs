using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface IOutboxEventRepository
{
    Task<IList<Guid>> GetPendingIdsAsync(int batchSize, CancellationToken cancellationToken);
    Task<OutboxEventEntity?> TryLockSingleAsync(Guid eventId, CancellationToken cancellationToken);
}
