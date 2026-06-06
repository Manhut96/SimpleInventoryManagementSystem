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
