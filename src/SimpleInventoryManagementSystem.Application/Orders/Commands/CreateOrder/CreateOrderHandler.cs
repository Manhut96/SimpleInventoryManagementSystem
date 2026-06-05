using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Exceptions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;
using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    ISIMSDbContext dbContext,
    IUnitOfWork unitOfWork,
    IPricingCalculatorService pricingCalculator)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await LoadCustomerAsync(request.CustomerId, cancellationToken);
        var products = await LoadProductsAsync(request.Items, cancellationToken);
        var rawItems = BuildRawLineItems(request.Items, products);
        var pricedItems = pricingCalculator.Calculate(rawItems, customer.Location);
        DeductStock(request.Items, products);
        var order = CreateOrder(request.CustomerId, pricedItems);
        await PersistOrderAsync(order, cancellationToken);
        return MapToDto(order);
    }

    private async Task<Customer> LoadCustomerAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);

        if (customer is null)
            throw new CustomerNotFoundException(customerId);

        return customer;
    }

    private async Task<Dictionary<Guid, Product>> LoadProductsAsync(
        IReadOnlyList<OrderItemRequest> items,
        CancellationToken ct)
    {
        var productIds = items.Select(i => i.ProductId).ToList();

        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var productId in productIds)
        {
            if (!products.ContainsKey(productId))
                throw new ProductNotFoundException(productId);
        }

        return products;
    }

    private static IReadOnlyList<OrderLineItem> BuildRawLineItems(
        IReadOnlyList<OrderItemRequest> items,
        Dictionary<Guid, Product> products)
        => items
            .Select(i => new OrderLineItem(i.ProductId, i.Quantity, products[i.ProductId].Price))
            .ToList();

    private static void DeductStock(
        IReadOnlyList<OrderItemRequest> items,
        Dictionary<Guid, Product> products)
    {
        foreach (var item in items)
            products[item.ProductId].DeductStock(item.Quantity);
    }

    private static Order CreateOrder(Guid customerId, IReadOnlyList<PricedOrderLineItem> pricedItems)
    {
        var orderItems = BuildOrderItems(pricedItems);
        var totalAmount = pricedItems.Sum(p => p.FinalUnitPrice * p.Quantity);
        return Order.Create(customerId, orderItems, totalAmount, DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<OrderItem> BuildOrderItems(IReadOnlyList<PricedOrderLineItem> pricedItems)
        => pricedItems
            .Select(p => OrderItem.Create(p.ProductId, p.Quantity, p.UnitPrice, p.FinalUnitPrice))
            .ToList();

    private async Task PersistOrderAsync(Order order, CancellationToken ct)
    {
        dbContext.Orders.Add(order);
        EnqueueOrderCreatedEvent(order);
        await unitOfWork.CommitAsync(ct);
    }

    private void EnqueueOrderCreatedEvent(Order order)
        => unitOfWork.Enqueue(new OrderCreatedEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            DateTimeOffset.UtcNow));

    private static OrderDto MapToDto(Order order)
    {
        var items = order.Items
            .Select(i => new OrderItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.FinalUnitPrice))
            .ToList();

        return new OrderDto(order.Id, order.CustomerId, items, order.TotalAmount, order.PlacedAt);
    }
}
