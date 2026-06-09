using Microsoft.Extensions.Logging;
using SimpleInventoryManagementSystem.Application.Common;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Exceptions;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;
using SimpleInventoryManagementSystem.Domain.ValueObjects;

namespace SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    ITransactionScopeFactory transactionScopeFactory,
    ISIMSDbContext dbContext,
    IProductRepository productRepository,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    IDateTimeProvider dateTimeProvider,
    IPricingCalculatorService pricingCalculator,
    ILoggerFactory loggerFactory)
    : TransactionalHandlerBase<CreateOrderCommand, OrderDto>(transactionScopeFactory, dbContext, dateTimeProvider, loggerFactory)
{
    protected override async Task<OrderDto> HandleCoreAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await LoadCustomerAsync(request.CustomerId, cancellationToken);
        var products = await LoadProductsAsync(request.Products, cancellationToken);
        var rawItems = request.Products
            .Select(i => new OrderLineItem(i.ProductId, i.Quantity, products[i.ProductId].Price))
            .ToList();
        var pricedItems = pricingCalculator.Calculate(rawItems, customer.Location);
        foreach (var item in request.Products)
            products[item.ProductId].DeductStock(item.Quantity);
        var order = CreateOrder(request.CustomerId, pricedItems, DateTimeProvider.UtcNow);
        orderRepository.Add(order);
        WriteEvent(new OrderCreatedEvent(order.Id, order.CustomerId, order.TotalAmount, DateTimeProvider.UtcNow));
        return MapToDto(order);
    }

    private async Task<CustomerEntity> LoadCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer is null)
            throw new CustomerNotFoundException(customerId);

        return customer;
    }

    private async Task<Dictionary<Guid, ProductEntity>> LoadProductsAsync(
        IReadOnlyList<OrderItemRequest> items,
        CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.ProductId).ToList();

        var products = (await productRepository.GetForUpdateAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        foreach (var productId in productIds)
        {
            if (!products.ContainsKey(productId))
                throw new ProductNotFoundException(productId);
        }

        return products;
    }

    private static OrderEntity CreateOrder(
        Guid customerId,
        IReadOnlyList<PricedOrderLineItem> pricedItems,
        DateTimeOffset placedAt)
    {
        var orderItems = pricedItems
            .Select(p => OrderItem.Create(p.ProductId, p.Quantity, p.UnitPrice, p.FinalUnitPrice))
            .ToList();
        var totalAmount = pricedItems.Sum(p => p.FinalUnitPrice * p.Quantity);
        return OrderEntity.Create(customerId, orderItems, totalAmount, placedAt);
    }

    private static OrderDto MapToDto(OrderEntity order)
    {
        var items = order.Items
            .Select(i => new OrderItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.FinalUnitPrice))
            .ToList();

        return new OrderDto(order.Id, order.CustomerId, items, order.TotalAmount, order.PlacedAt);
    }
}
