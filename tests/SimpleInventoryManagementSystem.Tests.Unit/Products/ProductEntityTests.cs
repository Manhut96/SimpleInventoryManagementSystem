using FluentAssertions;
using SimpleInventoryManagementSystem.Domain.Entities;
using SimpleInventoryManagementSystem.Domain.Exceptions;

namespace SimpleInventoryManagementSystem.Tests.Unit.Products;

public sealed class ProductEntityTests
{
    private static ProductEntity CreateProduct(int stock)
        => ProductEntity.Create("Product", "Description", 10m, stock);

    [Fact]
    public void DeductStock_SufficientQuantity_DecreasesStock()
    {
        var product = CreateProduct(stock: 10);

        product.DeductStock(3);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public void DeductStock_ExactStock_SetsStockToZero()
    {
        var product = CreateProduct(stock: 5);

        product.DeductStock(5);

        product.Stock.Should().Be(0);
    }

    [Fact]
    public void DeductStock_InsufficientQuantity_ThrowsInsufficientStockException()
    {
        var product = CreateProduct(stock: 2);

        var act = () => product.DeductStock(10);

        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void DeductStock_ZeroQuantity_LeavesStockUnchanged()
    {
        var product = CreateProduct(stock: 5);

        product.DeductStock(0);

        product.Stock.Should().Be(5);
    }
}
