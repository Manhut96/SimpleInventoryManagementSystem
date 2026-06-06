using FluentAssertions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Tests.Unit.Pricing;

public sealed class VolumeDiscountStrategyTests
{
    private readonly VolumeDiscountStrategy _sut = new();

    private static PricingContext ContextWithQuantity(int totalQuantity)
        => new([new OrderLineItem(Guid.NewGuid(), totalQuantity, 10m)], DateTimeOffset.UtcNow);

    public static TheoryData<int, decimal?> VolumeBoundaries => new()
    {
        { 4,  null   },
        { 5,  0.10m  },
        { 9,  0.10m  },
        { 10, 0.20m  },
        { 49, 0.20m  },
        { 50, 0.30m  },
        { 51, 0.30m  },
    };

    [Theory]
    [MemberData(nameof(VolumeBoundaries))]
    public void TryGetDiscount_AtBoundary_ReturnsExpectedDiscount(int quantity, decimal? expectedDiscount)
    {
        var context = ContextWithQuantity(quantity);

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(expectedDiscount);
    }

    [Fact]
    public void TryGetDiscount_QuantitySpreadAcrossItems_CountsTotalQuantity()
    {
        var items = new List<OrderLineItem>
        {
            new(Guid.NewGuid(), 3, 10m),
            new(Guid.NewGuid(), 2, 20m),
        };
        var context = new PricingContext(items, DateTimeOffset.UtcNow);

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(0.10m);
    }
}
