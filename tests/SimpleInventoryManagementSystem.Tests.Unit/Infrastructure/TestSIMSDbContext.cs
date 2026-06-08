using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

public sealed class TestSIMSDbContext : DbContext, ISIMSDbContext
{
    public TestSIMSDbContext(DbContextOptions<TestSIMSDbContext> options) : base(options) { }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OutboxEventEntity> OutboxEvents => Set<OutboxEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(order =>
        {
            order.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
            order.OwnsMany<OrderItem>(o => o.Items);
        });
    }
}
