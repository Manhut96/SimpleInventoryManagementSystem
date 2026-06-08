using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface IOrderRepository
{
    void Add(OrderEntity order);
}
