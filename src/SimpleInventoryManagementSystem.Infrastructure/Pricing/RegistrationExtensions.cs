using Microsoft.Extensions.DependencyInjection;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Infrastructure.Services;

namespace SimpleInventoryManagementSystem.Infrastructure.Pricing;

public static class RegistrationExtensions
{
    public static IServiceCollection AddPricingServices(this IServiceCollection services)
    {
        services.AddSingleton<IDiscountStrategy, VolumeDiscountStrategy>();
        services.AddSingleton<IDiscountStrategy, BlackFridayDiscountStrategy>();
        services.AddSingleton<IDiscountStrategy, HolidaySaleDiscountStrategy>();
        services.AddSingleton<IPricingCalculatorService, PricingCalculatorService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
