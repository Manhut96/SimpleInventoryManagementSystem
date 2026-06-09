using FluentAssertions;
using NSubstitute;
using SimpleInventoryManagementSystem.Domain.Enums;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Tests.Unit.Pricing;

public sealed class PricingCalculatorServiceTests
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private PricingCalculatorService BuildService(DateTimeOffset orderDate)
    {
        _dateTimeProvider.UtcNow.Returns(orderDate);
        return new PricingCalculatorService(
            [new VolumeDiscountStrategy(new VolumeDiscountOptions()), new BlackFridayDiscountStrategy(), new HolidaySaleDiscountStrategy()],
            _dateTimeProvider);
    }

    private static DateTimeOffset RegularDate => new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
    private static DateTimeOffset HolidayDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Black Friday 2024 = Nov 29 (1st Thursday = Nov 7, +22 days = Nov 29)
    private static DateTimeOffset BlackFridayDate => new(2024, 11, 29, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Calculate_NoDiscountApplicable_ReturnOriginalPrice()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 1, 50m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 50m);
    }

    [Fact]
    public void Calculate_VolumeDiscountQty5_Applies10PercentDiscount()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 5, 10m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 9m);
    }

    [Fact]
    public void Calculate_UsLocationNoDiscount_ReturnsOriginalPrice()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 1, 100m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 100m);
    }

    [Fact]
    public void Calculate_EuropeLocationNoDiscount_AppliesMultiplier()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 1, 100m) };

        var result = sut.Calculate(items, Location.Europe);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 115m);
    }

    [Fact]
    public void Calculate_VolumeDiscountWithEurope_AppliesDiscountThenMultiplier()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 5, 10m) };

        var result = sut.Calculate(items, Location.Europe);

        // 10% volume discount -> $9; Europe * 1.15 -> $10.35
        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 10.35m);
    }

    [Fact]
    public void Calculate_HolidayWinsOverVolume_AppliesHolidayOnlyToMostExpensiveItem()
    {
        // Holiday saves: $200 * 0.15 = $30; volume (qty=1) saves: $0 -> holiday wins
        var sut = BuildService(HolidayDate);
        var expensiveId = Guid.NewGuid();
        var cheapId = Guid.NewGuid();
        var items = new List<OrderLineItem>
        {
            new(expensiveId, 1, 200m),
            new(cheapId,     1, 50m),
        };

        var result = sut.Calculate(items, Location.Us);

        result.First(i => i.ProductId == expensiveId).FinalUnitPrice.Should().Be(170m);  // 200 * 0.85
        result.First(i => i.ProductId == cheapId).FinalUnitPrice.Should().Be(50m);        // no discount
    }

    [Fact]
    public void Calculate_VolumeWinsOverHoliday_AppliesVolumeToAllItems()
    {
        // Holiday saves: $1 * 0.15 = $0.15; volume (qty=10) saves: $10 * 0.20 = $2 -> volume wins
        var sut = BuildService(HolidayDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 10, 1m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 0.80m);
    }

    [Fact]
    public void Calculate_AsiaLocationWithVolumeDiscount_AppliesDiscountThenMultiplier()
    {
        var sut = BuildService(RegularDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 10, 100m) };

        var result = sut.Calculate(items, Location.Asia);

        // 20% volume discount -> $80; Asia * 1.05 -> $84
        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 84m);
    }

    [Fact]
    public void Calculate_BlackFridayWinsOverVolumeTier2_AppliesBlackFridayToAllItems()
    {
        // BF saves: (10 * $100) * 0.25 = $250; volume tier2 saves: $1000 * 0.20 = $200 -> BF wins
        var sut = BuildService(BlackFridayDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 10, 100m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 75m); // 100 * 0.75
    }

    [Fact]
    public void Calculate_VolumeTier3WinsOverBlackFriday_AppliesVolumeToAllItems()
    {
        // Volume tier3 saves: (50 * $100) * 0.30 = $1500; BF saves: $5000 * 0.25 = $1250 -> volume wins
        var sut = BuildService(BlackFridayDate);
        var productId = Guid.NewGuid();
        var items = new List<OrderLineItem> { new(productId, 50, 100m) };

        var result = sut.Calculate(items, Location.Us);

        result.Should().ContainSingle(i => i.ProductId == productId && i.FinalUnitPrice == 70m); // 100 * 0.70
    }
}
