using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Enums;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Domain.Exceptions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;
using SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

namespace SimpleInventoryManagementSystem.Tests.Unit.Orders.Commands;

public sealed class CreateOrderHandlerTests : IDisposable
{
    private readonly TestSIMSDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPricingCalculatorService _pricingCalculator = Substitute.For<IPricingCalculatorService>();
    private readonly CreateOrderHandler _sut;

    public CreateOrderHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestSIMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TestSIMSDbContext(options);
        _sut = new CreateOrderHandler(_dbContext, _unitOfWork, _pricingCalculator);
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task<(Customer customer, Product product)> SeedAsync(decimal price = 10m, int stock = 100)
    {
        var customer = Customer.Create("Test Customer", "test@test.com", Location.Europe);
        var product = Product.Create("Product", "Description", price, stock);
        _dbContext.Customers.Add(customer);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return (customer, product);
    }

    private void SetupPricing(Guid productId, int quantity, decimal unitPrice, decimal finalUnitPrice)
    {
        _pricingCalculator
            .Calculate(Arg.Any<IReadOnlyList<OrderLineItem>>(), Arg.Any<Location>())
            .Returns([new PricedOrderLineItem(productId, quantity, unitPrice, finalUnitPrice)]);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeductStockFromProduct()
    {
        var (customer, product) = await SeedAsync(stock: 5);
        SetupPricing(product.Id, 2, 10m, 10m);

        await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 2)]), CancellationToken.None);

        product.Stock.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldEnqueueOrderCreatedEventWithCorrectTotalAmount()
    {
        var (customer, product) = await SeedAsync(stock: 10);
        SetupPricing(product.Id, 2, 10m, 8m);

        await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 2)]), CancellationToken.None);

        _unitOfWork.Received(1).Enqueue(Arg.Is<OrderCreatedEvent>(e =>
            e.CustomerId == customer.Id &&
            e.TotalAmount == 16m));
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnOrderDtoWithCorrectTotalAmount()
    {
        var (customer, product) = await SeedAsync(price: 20m, stock: 10);
        SetupPricing(product.Id, 3, 20m, 18m);

        var result = await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 3)]), CancellationToken.None);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id);
        result.TotalAmount.Should().Be(54m);
        result.Items.Should().HaveCount(1);
        result.Items[0].FinalUnitPrice.Should().Be(18m);
        result.Items[0].Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCommitUnitOfWork()
    {
        var (customer, product) = await SeedAsync(stock: 5);
        SetupPricing(product.Id, 1, 10m, 10m);

        await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 1)]), CancellationToken.None);

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowCustomerNotFoundException()
    {
        await _sut.Invoking(h => h.Handle(
                new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(Guid.NewGuid(), 1)]),
                CancellationToken.None))
            .Should().ThrowAsync<CustomerNotFoundException>();
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowProductNotFoundException()
    {
        var (customer, _) = await SeedAsync();

        await _sut.Invoking(h => h.Handle(
                new CreateOrderCommand(customer.Id, [new OrderItemRequest(Guid.NewGuid(), 1)]),
                CancellationToken.None))
            .Should().ThrowAsync<ProductNotFoundException>();
    }
}
