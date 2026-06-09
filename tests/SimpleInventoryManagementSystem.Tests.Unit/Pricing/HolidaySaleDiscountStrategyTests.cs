using FluentAssertions;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Tests.Unit.Pricing;

public sealed class HolidaySaleDiscountStrategyTests
{
    private readonly HolidaySaleDiscountStrategy _sut = new();

    private PricingContext ContextWithDate(int month, int day)
        => new([new OrderLineItem(Guid.NewGuid(), 1, 100m)], new DateTimeOffset(2024, month, day, 0, 0, 0, TimeSpan.Zero));

    public static TheoryData<int, int> PolishHolidays => new()
    {
        { 1,  1  },  // New Year's Day
        { 1,  6  },  // Epiphany
        { 5,  1  },  // Labour Day
        { 5,  3  },  // Constitution Day
        { 8,  15 },  // Assumption of Mary
        { 11, 1  },  // All Saints' Day
        { 11, 11 },  // Independence Day
        { 12, 25 },  // Christmas Day
        { 12, 26 },  // Second Day of Christmas
    };

    public static TheoryData<int, int> DaysAdjacentToHolidays => new()
    {
        { 12, 24 },  // day before Christmas
        { 12, 27 },  // day after Second Christmas
        { 1,  2  },  // day after New Year's
        { 11, 10 },  // day before Independence Day
        { 11, 12 },  // day after Independence Day
        { 5,  2  },  // day after Labour Day
        { 8,  14 },  // day before Assumption of Mary
    };

    [Theory]
    [MemberData(nameof(PolishHolidays))]
    public void TryGetDiscount_OnHoliday_Returns15Percent(int month, int day)
    {
        var context = ContextWithDate(month, day);

        var result = _sut.TryGetDiscount(context);

        result.Should().Be(0.15m);
    }

    [Theory]
    [MemberData(nameof(DaysAdjacentToHolidays))]
    public void TryGetDiscount_DayAdjacentToHoliday_ReturnsNull(int month, int day)
    {
        var context = ContextWithDate(month, day);

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetDiscount_NonHolidayDate_ReturnsNull()
    {
        var context = ContextWithDate(6, 15);

        var result = _sut.TryGetDiscount(context);

        result.Should().BeNull();
    }
}
