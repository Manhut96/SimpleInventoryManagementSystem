using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

namespace SimpleInventoryManagementSystem.Tests.Unit.Products.Commands;

public sealed class CreateProductHandlerTests : IDisposable
{
    private readonly TestSIMSDbContext _dbContext;
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestSIMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TestSIMSDbContext(options);
        _sut = new CreateProductHandler(NoOpFactory(), _dbContext, NoOpDateTimeProvider(), NullLoggerFactory.Instance);
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

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddProductToDbContext()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        await _sut.Handle(command, CancellationToken.None);

        _dbContext.Products.Local.Should().ContainSingle(p =>
            p.Name == command.Name &&
            p.Description == command.Description &&
            p.Price == command.Price &&
            p.Stock == command.InitialStock);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldStageProductCreatedEventInOutbox()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        await _sut.Handle(command, CancellationToken.None);

        _dbContext.OutboxEvents.Local.Should().ContainSingle(e => e.EventType == "ProductCreatedEvent");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnProductDtoWithCorrectValues()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.Stock.Should().Be(command.InitialStock);
        result.Id.Should().NotBeEmpty();
    }
}
