using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(SIMSDbContext dbContext) : IOrderRepository
{
    public void Add(OrderEntity order) => dbContext.Orders.Add(order);
}
