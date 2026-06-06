using System.ComponentModel.DataAnnotations;

namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

/// <summary>Request body for creating a new product in the catalog.</summary>
/// <param name="Name">Product display name. Max 50 characters.</param>
/// <param name="Description">Short product description. Max 50 characters.</param>
/// <param name="Price">Unit price in USD. Must be greater than 0.</param>
/// <param name="Stock">Initial stock quantity. Must be 0 or greater.</param>
public record CreateProductRequest(
    [property: Required, MaxLength(50)] string Name,
    [property: Required, MaxLength(50)] string Description,
    [property: Required, Range(0.01, double.MaxValue)] decimal Price,
    [property: Required, Range(0, int.MaxValue)] int Stock
);
