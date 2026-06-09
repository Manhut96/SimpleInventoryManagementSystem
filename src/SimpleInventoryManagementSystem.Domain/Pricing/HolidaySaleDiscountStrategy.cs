using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class HolidaySaleDiscountStrategy : IDiscountStrategy
{
    private static readonly HashSet<PolishHoliday> Holidays =
    [
        new(1, 1), new(1, 6), new(5, 1), new(5, 3), new(8, 15),
        new(11, 1), new(11, 11), new(12, 25), new(12, 26)
    ];

    public decimal? TryGetDiscount(PricingContext context)
    {
        var orderDate = context.OrderDate;
        return Holidays.Contains(new PolishHoliday(orderDate.Month, orderDate.Day)) ? 0.15m : null;
    }

    public decimal ComputeSavings(decimal discountPct, decimal totalOrderValue, decimal maxUnitPrice)
        => maxUnitPrice * discountPct;

    public decimal GetFinalUnitPrice(OrderLineItem item, decimal discountPct, decimal maxUnitPrice)
        => item.UnitPrice == maxUnitPrice ? item.UnitPrice * (1 - discountPct) : item.UnitPrice;
}
