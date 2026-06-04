namespace SimpleInventoryManagementSystem.Domain.Exceptions;

public sealed class InsufficientStockException(Guid productId, int requested, int available)
    : DomainException($"Insufficient stock for product {productId}: requested {requested}, available {available}")
{
    public Guid ProductId { get; } = productId;
    public int Requested { get; } = requested;
    public int Available { get; } = available;
}
