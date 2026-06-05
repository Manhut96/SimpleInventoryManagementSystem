using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

public sealed class TestSIMSDbContext : DbContext, ISIMSDbContext
{
    public TestSIMSDbContext(DbContextOptions<TestSIMSDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            order.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
            order.OwnsMany<OrderItem>(o => o.Items);
        });
    }
}
