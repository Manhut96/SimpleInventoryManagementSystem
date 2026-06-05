using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;
using SimpleInventoryManagementSystem.Domain.Events;
using SimpleInventoryManagementSystem.Tests.Unit.Infrastructure;

namespace SimpleInventoryManagementSystem.Tests.Unit.Products.Commands;

public sealed class CreateProductHandlerTests : IDisposable
{
    private readonly TestSIMSDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestSIMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TestSIMSDbContext(options);
        _sut = new CreateProductHandler(_dbContext, _unitOfWork);
    }

    public void Dispose() => _dbContext.Dispose();

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
    public async Task Handle_ValidCommand_ShouldEnqueueProductCreatedEvent()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        await _sut.Handle(command, CancellationToken.None);

        _unitOfWork.Received(1).Enqueue(Arg.Is<ProductCreatedEvent>(e =>
            e.Name == command.Name &&
            e.Price == command.Price &&
            e.Stock == command.InitialStock));
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCommitUnitOfWork()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        await _sut.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
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
