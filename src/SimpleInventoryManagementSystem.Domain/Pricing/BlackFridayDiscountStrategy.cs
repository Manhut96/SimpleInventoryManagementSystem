namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class BlackFridayDiscountStrategy : IDiscountStrategy
{
    public decimal? TryGetDiscount(PricingContext context)
    {
        var orderDate = context.OrderDate;

        if (orderDate.Month != 11)
            return null;

        if (!IsLastFridayOfNovember(orderDate))
            return null;

        return 0.25m;
    }

    private static bool IsLastFridayOfNovember(DateTimeOffset date)
    {
        if (date.DayOfWeek != DayOfWeek.Friday)
            return false;

        // It's the last Friday if adding 7 days would fall in December
        return date.AddDays(7).Month == 12;
    }
}
