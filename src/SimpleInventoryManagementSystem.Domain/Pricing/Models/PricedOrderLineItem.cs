namespace SimpleInventoryManagementSystem.Domain.Pricing.Models;

public record PricedOrderLineItem(Guid ProductId, int Quantity, decimal UnitPrice, decimal FinalUnitPrice);
