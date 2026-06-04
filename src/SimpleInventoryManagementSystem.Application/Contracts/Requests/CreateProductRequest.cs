namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

public record CreateProductRequest(string Name, string Description, decimal Price, int Stock);
