using SimpleInventoryManagementSystem.Application.Products.Models;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface IProductRepository
{
    void Add(ProductEntity product);
    Task<ProductsPage> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<List<ProductEntity>> GetForUpdateAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
}
