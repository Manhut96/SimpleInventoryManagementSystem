using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
