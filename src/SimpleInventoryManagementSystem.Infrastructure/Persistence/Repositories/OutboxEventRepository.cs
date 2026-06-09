using Microsoft.EntityFrameworkCore;
using Npgsql;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class OutboxEventRepository(SIMSDbContext dbContext) : IOutboxEventRepository
{
    public async Task<IList<Guid>> GetPendingIdsAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await dbContext.OutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<OutboxEventEntity?> TryLockSingleAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventIdParam = new NpgsqlParameter("eventId", eventId);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            SELECT 1 FROM events.tbl_outbox
            WHERE "Id" = @eventId AND "ProcessedAt" IS NULL
            FOR UPDATE SKIP LOCKED
            """,
            [eventIdParam], cancellationToken);

        return await dbContext.OutboxEvents.FindAsync([eventId], cancellationToken);
    }
}
