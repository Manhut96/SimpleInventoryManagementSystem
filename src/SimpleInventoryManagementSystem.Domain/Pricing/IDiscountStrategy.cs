using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public interface IDiscountStrategy
{
    decimal? TryGetDiscount(PricingContext context);

    decimal ComputeSavings(decimal discountPct, decimal totalOrderValue, decimal maxUnitPrice)
        => totalOrderValue * discountPct;

    decimal GetFinalUnitPrice(OrderLineItem item, decimal discountPct, decimal maxUnitPrice)
        => item.UnitPrice * (1 - discountPct);
}
