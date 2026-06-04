namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

public record OrderDto(Guid Id, Guid CustomerId, IReadOnlyList<OrderItemDto> Items, decimal TotalAmount, DateTimeOffset PlacedAt);
