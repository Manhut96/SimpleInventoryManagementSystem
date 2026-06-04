namespace SimpleInventoryManagementSystem.Domain.Pricing.Models;

public record DiscountWinner(IDiscountStrategy? Strategy, decimal? Pct);
