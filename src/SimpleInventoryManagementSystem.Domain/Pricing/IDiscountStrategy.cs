namespace SimpleInventoryManagementSystem.Domain.Pricing;

public interface IDiscountStrategy
{
    decimal? TryGetDiscount(PricingContext context);
}
