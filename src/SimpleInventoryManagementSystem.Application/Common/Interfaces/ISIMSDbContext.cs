using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ISIMSDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<CustomerEntity> Customers { get; }
    DbSet<OrderEntity> Orders { get; }
    DbSet<OutboxEventEntity> OutboxEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<List<ProductEntity>> GetProductsForUpdateAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
}
