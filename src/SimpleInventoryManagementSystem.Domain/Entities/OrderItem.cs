namespace SimpleInventoryManagementSystem.Domain.Entities;

public class OrderItem
{
    private OrderItem() { }

    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal FinalUnitPrice { get; private set; }

    public static OrderItem Create(Guid productId, int quantity, decimal unitPrice, decimal finalUnitPrice)
    {
        return new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            FinalUnitPrice = finalUnitPrice
        };
    }
}
