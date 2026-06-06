using System.ComponentModel.DataAnnotations;

namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

/// <summary>Request body for placing a new order.</summary>
/// <param name="CustomerId">ID of the customer placing the order.</param>
/// <param name="Items">List of products and quantities to order. At least one item required.</param>
public record CreateOrderRequest(
    [property: Required] Guid CustomerId,
    [property: Required, MinLength(1)] IReadOnlyList<OrderItemRequest> Items
);
