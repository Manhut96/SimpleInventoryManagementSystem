using System.ComponentModel.DataAnnotations;

namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

/// <summary>A single line item in an order.</summary>
/// <param name="ProductId">ID of the product to order.</param>
/// <param name="Quantity">Number of units to order. Must be at least 1.</param>
public record OrderItemRequest(
    [Required] Guid ProductId,
    [Required, Range(1, int.MaxValue)] int Quantity
);
