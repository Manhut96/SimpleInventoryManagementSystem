namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

public record ProductDto(Guid Id, string Name, string Description, decimal Price, int Stock);
