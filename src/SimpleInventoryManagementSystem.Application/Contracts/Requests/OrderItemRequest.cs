namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

public record OrderItemRequest(Guid ProductId, int Quantity);
