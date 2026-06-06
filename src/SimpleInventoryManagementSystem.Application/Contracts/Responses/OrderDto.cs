namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

/// <summary>Represents a placed order.</summary>
/// <param name="Id">Unique identifier of the order.</param>
/// <param name="CustomerId">ID of the customer who placed the order.</param>
/// <param name="Items">Line items included in the order.</param>
/// <param name="TotalAmount">Final total after discounts and location-based pricing.</param>
/// <param name="PlacedAt">Timestamp when the order was placed.</param>
public record OrderDto(Guid Id, Guid CustomerId, IReadOnlyList<OrderItemDto> Items, decimal TotalAmount, DateTimeOffset PlacedAt);
