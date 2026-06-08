using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(SIMSDbContext dbContext) : ICustomerRepository
{
    public Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
