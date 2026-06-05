namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class HolidaySaleDiscountStrategy : IDiscountStrategy
{
    private static readonly HashSet<(int Month, int Day)> Holidays =
    [
        (1, 1), (1, 6), (5, 1), (5, 3), (8, 15), (11, 1), (11, 11), (12, 25), (12, 26)
    ];

    public decimal? TryGetDiscount(PricingContext context)
    {
        var orderDate = context.OrderDate;
        return Holidays.Contains((orderDate.Month, orderDate.Day)) ? 0.15m : null;
    }
}
