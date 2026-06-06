namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

/// <summary>Represents a single line item in an order response.</summary>
/// <param name="ProductId">ID of the ordered product.</param>
/// <param name="Quantity">Number of units ordered.</param>
/// <param name="UnitPrice">Original unit price before discounts.</param>
/// <param name="FinalUnitPrice">Unit price after discounts and location-based pricing.</param>
public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal FinalUnitPrice);
