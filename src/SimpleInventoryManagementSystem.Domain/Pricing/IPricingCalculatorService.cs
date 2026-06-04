using SimpleInventoryManagementSystem.Domain.Enums;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public interface IPricingCalculatorService
{
    IReadOnlyList<PricedOrderLineItem> Calculate(
        IReadOnlyList<OrderLineItem> items,
        Location location);
}
