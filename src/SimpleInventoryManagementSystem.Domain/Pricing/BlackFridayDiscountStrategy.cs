namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class BlackFridayDiscountStrategy : IDiscountStrategy
{
    public decimal? TryGetDiscount(PricingContext context)
    {
        return IsBlackFriday(context.OrderDate) ? 0.25m : null;
    }

    private static bool IsBlackFriday(DateTimeOffset date)
    {
        if (date.Month != 11 || date.DayOfWeek != DayOfWeek.Friday)
            return false;

        var firstThursday = new DateTimeOffset(date.Year, 11, 1, 0, 0, 0, date.Offset);
        while (firstThursday.DayOfWeek != DayOfWeek.Thursday)
            firstThursday = firstThursday.AddDays(1);
        var blackFriday = firstThursday.AddDays(3 * 7 + 1); // 3 weeks to 4th Thursday + 1 day to Friday

        return date.Date == blackFriday.Date;
    }
}
