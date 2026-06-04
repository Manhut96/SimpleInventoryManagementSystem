namespace SimpleInventoryManagementSystem.Domain.Exceptions;

public sealed class ProductNotFoundException(Guid productId)
    : DomainException($"Product {productId} was not found");
