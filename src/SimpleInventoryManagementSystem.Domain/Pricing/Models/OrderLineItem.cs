namespace SimpleInventoryManagementSystem.Domain.Pricing.Models;

public record OrderLineItem(Guid ProductId, int Quantity, decimal UnitPrice);
