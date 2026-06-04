using SimpleInventoryManagementSystem.Domain.Pricing.Models;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public record PricingContext(
    IReadOnlyList<OrderLineItem> Items,
    DateTimeOffset OrderDate);
