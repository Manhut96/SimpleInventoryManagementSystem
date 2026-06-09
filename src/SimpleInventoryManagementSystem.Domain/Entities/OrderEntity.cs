using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class OrderEntity
{
    private OrderEntity() { }

    private List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    public static OrderEntity Create(Guid customerId, IReadOnlyList<OrderItem> items, decimal totalAmount, DateTimeOffset placedAt)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID must not be empty.", nameof(customerId));
        if (items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(items));
        if (totalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount cannot be negative.");

        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TotalAmount = totalAmount,
            PlacedAt = placedAt
        };
        order._items.AddRange(items);
        return order;
    }
}
