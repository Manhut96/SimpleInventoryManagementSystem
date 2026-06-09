using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ISIMSDbContext
{
    DbSet<OutboxEventEntity> OutboxEvents { get; }
    DbSet<ProductEntity> Products { get; }
    DbSet<CustomerEntity> Customers { get; }
    DbSet<OrderEntity> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
