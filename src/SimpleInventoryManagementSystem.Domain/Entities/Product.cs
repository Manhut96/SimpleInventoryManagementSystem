using SimpleInventoryManagementSystem.Domain.Exceptions;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class Product
{
    private Product() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public static Product Create(string name, string description, decimal price, int initialStock)
    {
        return new Product
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
