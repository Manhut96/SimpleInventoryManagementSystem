using FluentAssertions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Tests.Unit.Pricing;

public sealed class BlackFridayDiscountStrategyTests
{
    private readonly BlackFridayDiscountStrategy _sut = new();

    private PricingContext ContextWithDate(DateTimeOffset orderDate)
        => new([new OrderLineItem(Guid.NewGuid(), 1, 100m)], orderDate);

    [Fact]
    public void TryGetDiscount_LastFridayOfNovember2024_Returns25Percent()
    {
        var context = ContextWithDate(new DateTimeOffset(2024, 11, 29, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(0.25m);
    }

    [Fact]
    public void TryGetDiscount_SecondToLastFridayOfNovember_ReturnsNull()
    {
        var context = ContextWithDate(new DateTimeOffset(2024, 11, 22, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetDiscount_NonNovemberDate_ReturnsNull()
    {
        var context = ContextWithDate(new DateTimeOffset(2024, 12, 27, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetDiscount_NovemberDateNotFriday_ReturnsNull()
    {
        var context = ContextWithDate(new DateTimeOffset(2024, 11, 28, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }
}
