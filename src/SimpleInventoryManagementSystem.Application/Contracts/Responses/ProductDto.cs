namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

/// <summary>Represents a product in the catalog.</summary>
/// <param name="Id">Unique identifier of the product.</param>
/// <param name="Name">Display name of the product.</param>
/// <param name="Description">Short description of the product.</param>
/// <param name="Price">Unit price in USD.</param>
/// <param name="Stock">Current available stock quantity.</param>
public record ProductDto(Guid Id, string Name, string Description, decimal Price, int Stock);
