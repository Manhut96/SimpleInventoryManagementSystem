using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Enums;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(SIMSDbContext db, ILogger logger)
    {
        if (await db.Customers.AnyAsync()) return;

        var customers = SeedCustomers();
        var products = SeedProducts();

        db.Customers.AddRange(customers);
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        logger.LogInformation("[DataSeeder] Seeded customers:");
        foreach (var customer in customers)
            logger.LogInformation("  {Name}: {CustomerId}", customer.Name, customer.Id);
    }

    private static List<CustomerEntity> SeedCustomers()
    {
        return
        [
            CustomerEntity.Create("Alice", "alice@example.com", Location.Us,     new Guid("11111111-0000-0000-0000-000000000001")),
            CustomerEntity.Create("Bob",   "bob@example.com",   Location.Europe,  new Guid("22222222-0000-0000-0000-000000000002")),
            CustomerEntity.Create("Carol", "carol@example.com", Location.Asia,    new Guid("33333333-0000-0000-0000-000000000003"))
        ];
    }

    private static List<ProductEntity> SeedProducts()
    {
        return
        [
            ProductEntity.Create("Laptop",      "Laptop computer",      1200m, 10),
            ProductEntity.Create("Monitor",     "Display monitor",       300m, 25),
            ProductEntity.Create("Keyboard",    "Mechanical keyboard",    80m, 50),
            ProductEntity.Create("Mouse",       "Wireless mouse",         40m, 100),
            ProductEntity.Create("Headphones",  "Over-ear headphones",   150m, 30)
        ];
    }
}
