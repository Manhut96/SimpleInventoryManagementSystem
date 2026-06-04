namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class VolumeDiscountStrategy : IDiscountStrategy
{
    public decimal? TryGetDiscount(PricingContext context)
    {
        var totalQuantity = context.Items.Sum(i => i.Quantity);

        return totalQuantity switch
        {
            >= 50 => 0.30m,
            >= 10 => 0.20m,
            >= 5  => 0.10m,
            _     => null
        };
    }
}
