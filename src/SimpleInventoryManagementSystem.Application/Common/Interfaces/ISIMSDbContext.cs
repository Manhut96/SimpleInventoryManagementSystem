using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ISIMSDbContext
{
    DbSet<OutboxEventEntity> OutboxEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
