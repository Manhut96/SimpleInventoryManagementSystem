using SimpleInventoryManagementSystem.Domain.Enums;

namespace SimpleInventoryManagementSystem.Domain.Pricing;

public static class LocationPricingService
{
    public static decimal GetMultiplier(Location location) => location switch
    {
        Location.Us     => 1.0m,
        Location.Europe => 1.15m,
        Location.Asia   => 1.05m,
        _               => 1.0m
    };
}
