using SimpleInventoryManagementSystem.Domain.Exceptions;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class ProductEntity
{
    private ProductEntity() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public static ProductEntity Create(string name, string description, decimal price, int initialStock)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
            throw new ArgumentException("Name must be between 1 and 50 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(description) || description.Length > 50)
            throw new ArgumentException("Description must be between 1 and 50 characters.", nameof(description));
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
        if (initialStock < 0)
            throw new ArgumentOutOfRangeException(nameof(initialStock), "Initial stock cannot be negative.");

        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Stock = initialStock
        };
    }

    public void DeductStock(int quantity)
    {
        if (quantity > Stock)
            throw new InsufficientStockException(Id, quantity, Stock);

        Stock -= quantity;
    }
}
