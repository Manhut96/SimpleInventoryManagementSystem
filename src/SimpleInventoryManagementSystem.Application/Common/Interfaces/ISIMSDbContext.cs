using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ISIMSDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OutboxEvent> OutboxEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
