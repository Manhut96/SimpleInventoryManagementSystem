using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Products.Models;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(SIMSDbContext dbContext) : IProductRepository
{
    public void Add(ProductEntity product) => dbContext.Products.Add(product);

    public async Task<ProductsPage> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await dbContext.Products.CountAsync(cancellationToken);
        var items = await dbContext.Products
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ProductsPage(totalCount, items);
    }

    public async Task<List<ProductEntity>> GetForUpdateAsync(
        IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var idsParam = new NpgsqlParameter("ids", ids.ToArray())
        {
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
        };
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM catalog.tbl_products WHERE id = ANY(@ids) FOR UPDATE",
            [idsParam], cancellationToken);

        return await dbContext.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}
