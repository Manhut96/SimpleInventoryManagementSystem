using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence;

public class SIMSDbContext : DbContext, ISIMSDbContext
{
    public SIMSDbContext(DbContextOptions<SIMSDbContext> options) : base(options) { }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OutboxEventEntity> OutboxEvents => Set<OutboxEventEntity>();

    public async Task<List<ProductEntity>> GetProductsForUpdateAsync(
        IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var idsParam = new NpgsqlParameter("ids", ids.ToArray())
        {
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
        };
        await Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM catalog.tbl_products WHERE id = ANY(@ids) FOR UPDATE",
            [idsParam], cancellationToken);

        return await Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SIMSDbContext).Assembly);
    }
}
