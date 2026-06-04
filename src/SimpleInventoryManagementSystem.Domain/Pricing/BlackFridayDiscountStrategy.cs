using SimpleInventoryManagementSystem.Domain.Interfaces;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class BlackFridayDiscountStrategy(IDateTimeProvider dateTimeProvider) : IDiscountStrategy
{
    public decimal? TryGetDiscount(PricingContext context)
    {
        var today = dateTimeProvider.UtcNow;

        if (today.Month != 11)
            return null;

        if (!IsLastFridayOfNovember(today))
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
