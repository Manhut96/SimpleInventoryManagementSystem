using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Enums;
using SimpleInventoryManagementSystem.Domain.Exceptions;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;
using SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

namespace SimpleInventoryManagementSystem.Tests.Unit.Orders.Commands;

public sealed class CreateOrderHandlerTests : IDisposable
{
    private readonly TestSIMSDbContext _dbContext;
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IPricingCalculatorService _pricingCalculator = Substitute.For<IPricingCalculatorService>();
    private readonly CreateOrderHandler _sut;

    public CreateOrderHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestSIMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TestSIMSDbContext(options);
        _sut = new CreateOrderHandler(
            NoOpFactory(), _dbContext, _productRepository, _customerRepository, _orderRepository,
            NoOpDateTimeProvider(), _pricingCalculator, NullLoggerFactory.Instance);
    }

    public void Dispose() => _dbContext.Dispose();

    private static ITransactionScopeFactory NoOpFactory()
    {
        var factory = Substitute.For<ITransactionScopeFactory>();
        var scope = Substitute.For<ITransactionScope>();
        scope.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        scope.DisposeAsync().Returns(ValueTask.CompletedTask);
        factory.BeginAsync(Arg.Any<CancellationToken>()).Returns(scope);
        return factory;
    }

    private static IDateTimeProvider NoOpDateTimeProvider()
    {
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
        return dateTimeProvider;
    }

    private static CustomerEntity CreateCustomer()
        => CustomerEntity.Create("Test Customer", "test@test.com", Location.Europe);

    private static ProductEntity CreateProduct(decimal price = 10m, int stock = 100)
        => ProductEntity.Create("Product", "Description", price, stock);

    private void SetupCustomer(CustomerEntity customer)
        => _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

    private void SetupProducts(List<ProductEntity> products)
        => _productRepository.GetForUpdateAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>()).Returns(products);

    private void SetupPricing(Guid productId, int quantity, decimal unitPrice, decimal finalUnitPrice)
        => _pricingCalculator
            .Calculate(Arg.Any<IReadOnlyList<OrderLineItem>>(), Arg.Any<Location>())
            .Returns([new PricedOrderLineItem(productId, quantity, unitPrice, finalUnitPrice)]);

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeductStockFromProduct()
    {
        var customer = CreateCustomer();
        var product = CreateProduct(stock: 5);
        SetupCustomer(customer);
        SetupProducts([product]);
        SetupPricing(product.Id, 2, 10m, 10m);

        await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 2)]), CancellationToken.None);

        product.Stock.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldStageOrderCreatedEventInOutbox()
    {
        var customer = CreateCustomer();
        var product = CreateProduct(stock: 10);
        SetupCustomer(customer);
        SetupProducts([product]);
        SetupPricing(product.Id, 2, 10m, 8m);

        await _sut.Handle(new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 2)]), CancellationToken.None);

        _dbContext.OutboxEvents.Local.Should().ContainSingle(e => e.EventType == "OrderCreatedEvent");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnOrderDtoWithCorrectTotalAmount()
    {
        var customer = CreateCustomer();
        var product = CreateProduct(price: 20m, stock: 10);
        SetupCustomer(customer);
        SetupProducts([product]);
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
        var customer = CreateCustomer();
        SetupCustomer(customer);
        SetupProducts([]);

        await _sut.Invoking(h => h.Handle(
                new CreateOrderCommand(customer.Id, [new OrderItemRequest(Guid.NewGuid(), 1)]),
                CancellationToken.None))
            .Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsInsufficientStockException()
    {
        var customer = CreateCustomer();
        var product = CreateProduct(stock: 2);
        SetupCustomer(customer);
        SetupProducts([product]);

        await _sut.Invoking(h => h.Handle(
                new CreateOrderCommand(customer.Id, [new OrderItemRequest(product.Id, 100)]),
                CancellationToken.None))
            .Should().ThrowAsync<InsufficientStockException>();
    }
}
