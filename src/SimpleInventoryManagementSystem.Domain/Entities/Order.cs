using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Domain.Entities;

public class Order
{
    private Order() { }

    private List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    public static Order Create(Guid customerId, IReadOnlyList<OrderItem> items, decimal totalAmount, DateTimeOffset placedAt)
    {
        var order = new Order
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
