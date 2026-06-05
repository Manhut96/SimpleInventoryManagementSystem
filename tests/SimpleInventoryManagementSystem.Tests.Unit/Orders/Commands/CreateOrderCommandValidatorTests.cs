using FluentAssertions;
using FluentValidation.TestHelper;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

namespace SimpleInventoryManagementSystem.Tests.Unit.Orders.Commands;

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _sut = new();

    private static OrderItemRequest ValidItem()
        => new(Guid.NewGuid(), 1);

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), [ValidItem()]);

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldFail()
    {
        var command = new CreateOrderCommand(Guid.Empty, [ValidItem()]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_EmptyItems_ShouldFail()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), []);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validate_ItemWithEmptyProductId_ShouldFail()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(Guid.Empty, 1)]);

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ItemWithZeroOrNegativeQuantity_ShouldFail(int quantity)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(Guid.NewGuid(), quantity)]);

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeFalse();
    }
}
