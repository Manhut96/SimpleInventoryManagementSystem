using MediatR;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(Guid CustomerId, IReadOnlyList<OrderItemRequest> Items) : IRequest<OrderDto>;
