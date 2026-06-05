namespace SimpleInventoryManagementSystem.Domain.Pricing.Models;

internal record DiscountWinner(IDiscountStrategy? Strategy, decimal? Pct);
