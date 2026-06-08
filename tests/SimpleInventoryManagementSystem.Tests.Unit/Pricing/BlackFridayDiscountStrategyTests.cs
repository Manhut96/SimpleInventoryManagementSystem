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

    [Fact]
    public void TryGetDiscount_BlackFriday2018_Returns25Percent()
    {
        // 2018: Nov 1 = Thursday → 4th Thursday = Nov 22, Black Friday = Nov 23
        // "Last Friday" algorithm would give Nov 30 — this test catches the regression
        var context = ContextWithDate(new DateTimeOffset(2018, 11, 23, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(0.25m);
    }

    [Fact]
    public void TryGetDiscount_LastFridayOfNovember2018NotBlackFriday_ReturnsNull()
    {
        // Nov 30, 2018 is the last Friday of November but NOT Black Friday (Nov 23 is)
        var context = ContextWithDate(new DateTimeOffset(2018, 11, 30, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetDiscount_BlackFriday2025_Returns25Percent()
    {
        // 2025: Nov 1 = Saturday → 4th Thursday = Nov 27, Black Friday = Nov 28
        var context = ContextWithDate(new DateTimeOffset(2025, 11, 28, 0, 0, 0, TimeSpan.Zero));

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(0.25m);
    }
}
