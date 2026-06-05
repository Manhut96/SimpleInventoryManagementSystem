using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;

namespace SimpleInventoryManagementSystem.Tests.Unit.Products.Commands;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateProductCommand("Coffee Mug", "A nice mug", 9.99m, 10);

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldFail(string? name)
    {
        var command = new CreateProductCommand(name!, "Description", 9.99m, 10);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceeds50Characters_ShouldFail()
    {
        var command = new CreateProductCommand(new string('A', 51), "Description", 9.99m, 10);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyDescription_ShouldFail(string? description)
    {
        var command = new CreateProductCommand("Name", description!, 9.99m, 10);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionExceeds50Characters_ShouldFail()
    {
        var command = new CreateProductCommand("Name", new string('A', 51), 9.99m, 10);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceNotGreaterThanZero_ShouldFail(decimal price)
    {
        var command = new CreateProductCommand("Name", "Description", price, 10);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_NegativeStock_ShouldFail()
    {
        var command = new CreateProductCommand("Name", "Description", 9.99m, -1);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InitialStock);
    }

    [Fact]
    public void Validate_ZeroStock_ShouldPass()
    {
        var command = new CreateProductCommand("Name", "Description", 9.99m, 0);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.InitialStock);
    }
}
