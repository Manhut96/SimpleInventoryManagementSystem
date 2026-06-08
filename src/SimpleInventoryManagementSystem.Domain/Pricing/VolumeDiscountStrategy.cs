using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public sealed class VolumeDiscountStrategy(VolumeDiscountOptions options) : IDiscountStrategy
{
    public decimal? TryGetDiscount(PricingContext context)
    {
        var totalQuantity = context.Items.Sum(i => i.Quantity);

        if (totalQuantity >= options.Tier3MinQuantity) return options.Tier3Rate;
        if (totalQuantity >= options.Tier2MinQuantity) return options.Tier2Rate;
        if (totalQuantity >= options.Tier1MinQuantity) return options.Tier1Rate;
        return null;
    }
}
