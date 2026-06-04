using SimpleInventoryManagementSystem.Domain.Enums;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class PricingCalculatorService(
    IEnumerable<IDiscountStrategy> strategies,
    IDateTimeProvider dateTimeProvider) : IPricingCalculatorService
{
    public IReadOnlyList<PricedOrderLineItem> Calculate(IReadOnlyList<OrderLineItem> items, Location location)
    {
        var context = new PricingContext(items, dateTimeProvider.UtcNow);
        var totalOrderValue = items.Sum(i => i.Quantity * i.UnitPrice);
        var maxUnitPrice = items.Max(i => i.UnitPrice);
        var locationMultiplier = LocationPricingService.GetMultiplier(location);

        var (winner, winnerPct) = SelectWinner(context, totalOrderValue, maxUnitPrice);

        return items
            .Select(item => PriceItem(item, winner, winnerPct, maxUnitPrice, locationMultiplier))
            .ToList();
    }

    private (IDiscountStrategy? Winner, decimal? Pct) SelectWinner(
        PricingContext context, decimal totalOrderValue, decimal maxUnitPrice)
    {
        IDiscountStrategy? winner = null;
        decimal? winnerPct = null;
        var bestSavings = 0m;

        foreach (var strategy in strategies)
        {
            var discount = strategy.TryGetDiscount(context);
            if (discount is null) continue;

            var savings = ComputeSavings(strategy, discount.Value, totalOrderValue, maxUnitPrice);
            if (savings > bestSavings)
            {
                bestSavings = savings;
                winner = strategy;
                winnerPct = discount;
            }
        }

        return (winner, winnerPct);
    }

    private static decimal ComputeSavings(
        IDiscountStrategy strategy, decimal discountPct, decimal totalOrderValue, decimal maxUnitPrice)
        => strategy is HolidaySaleDiscountStrategy
            ? maxUnitPrice * discountPct
            : totalOrderValue * discountPct;

    private static PricedOrderLineItem PriceItem(
        OrderLineItem item, IDiscountStrategy? winner, decimal? winnerPct,
        decimal maxUnitPrice, decimal locationMultiplier)
    {
        var discounted = ApplyStrategyDiscount(item, winner, winnerPct, maxUnitPrice);
        return new PricedOrderLineItem(item.ProductId, item.Quantity, item.UnitPrice, discounted * locationMultiplier);
    }

    private static decimal ApplyStrategyDiscount(
        OrderLineItem item, IDiscountStrategy? winner, decimal? winnerPct, decimal maxUnitPrice)
    {
        if (winner is HolidaySaleDiscountStrategy && item.UnitPrice == maxUnitPrice)
            return item.UnitPrice * (1 - winnerPct!.Value);

        if (winner is not null and not HolidaySaleDiscountStrategy)
            return item.UnitPrice * (1 - winnerPct!.Value);

        return item.UnitPrice;
    }
}
