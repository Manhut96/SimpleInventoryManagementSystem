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

        var winner = SelectWinner(context, totalOrderValue, maxUnitPrice);

        return items
            .Select(item => PriceItem(item, winner, maxUnitPrice, locationMultiplier))
            .ToList();
    }

    private DiscountWinner SelectWinner(
        PricingContext context, decimal totalOrderValue, decimal maxUnitPrice)
    {
        IDiscountStrategy? bestStrategy = null;
        decimal? bestPct = null;
        var bestSavings = 0m;

        foreach (var strategy in strategies)
        {
            var discount = strategy.TryGetDiscount(context);
            if (discount is null) continue;

            var savings = strategy.ComputeSavings(discount.Value, totalOrderValue, maxUnitPrice);
            if (savings > bestSavings)
            {
                bestSavings = savings;
                bestStrategy = strategy;
                bestPct = discount;
            }
        }

        return new DiscountWinner(bestStrategy, bestPct);
    }

    private static PricedOrderLineItem PriceItem(
        OrderLineItem item, DiscountWinner winner, decimal maxUnitPrice, decimal locationMultiplier)
    {
        var finalUnitPrice = winner.Strategy is not null
            ? winner.Strategy.GetFinalUnitPrice(item, winner.Pct!.Value, maxUnitPrice)
            : item.UnitPrice;
        return new PricedOrderLineItem(item.ProductId, item.Quantity, item.UnitPrice, finalUnitPrice * locationMultiplier);
    }
}
