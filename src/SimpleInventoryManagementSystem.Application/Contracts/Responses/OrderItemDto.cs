namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal FinalUnitPrice);
