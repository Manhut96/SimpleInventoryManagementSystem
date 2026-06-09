using FluentAssertions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Tests.Unit.Pricing;

public sealed class VolumeDiscountStrategyTests
{
    private static readonly VolumeDiscountOptions Options = new();
    private readonly VolumeDiscountStrategy _sut = new(Options);

    private static PricingContext ContextWithQuantity(int totalQuantity)
        => new([new OrderLineItem(Guid.NewGuid(), totalQuantity, 10m)], DateTimeOffset.UtcNow);

    public static TheoryData<int, decimal?> VolumeBoundaries => new()
    {
        { Options.Tier1MinQuantity - 1, null                },
        { Options.Tier1MinQuantity,     Options.Tier1Rate   },
        { Options.Tier2MinQuantity - 1, Options.Tier1Rate   },
        { Options.Tier2MinQuantity,     Options.Tier2Rate   },
        { Options.Tier3MinQuantity - 1, Options.Tier2Rate   },
        { Options.Tier3MinQuantity,     Options.Tier3Rate   },
        { Options.Tier3MinQuantity + 1, Options.Tier3Rate   },
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

        result.Should().Be(Options.Tier1Rate);
    }
}
