namespace SimpleInventoryManagementSystem.Domain.Pricing.Models;

public record VolumeDiscountOptions
{
    public int Tier1MinQuantity { get; init; } = 5;
    public int Tier2MinQuantity { get; init; } = 10;
    public int Tier3MinQuantity { get; init; } = 50;
    public decimal Tier1Rate { get; init; } = 0.10m;
    public decimal Tier2Rate { get; init; } = 0.20m;
    public decimal Tier3Rate { get; init; } = 0.30m;
}
