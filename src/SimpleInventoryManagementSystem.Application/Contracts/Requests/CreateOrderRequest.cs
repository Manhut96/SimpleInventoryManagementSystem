namespace SimpleInventoryManagementSystem.Application.Contracts.Requests;

public record CreateOrderRequest(Guid CustomerId, IReadOnlyList<OrderItemRequest> Items);
