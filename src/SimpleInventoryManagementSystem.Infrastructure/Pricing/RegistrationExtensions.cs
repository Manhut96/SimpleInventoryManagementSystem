using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleInventoryManagementSystem.Domain.Interfaces;
using SimpleInventoryManagementSystem.Domain.Pricing;
using SimpleInventoryManagementSystem.Domain.Pricing.Models;
using SimpleInventoryManagementSystem.Infrastructure.Services;

namespace SimpleInventoryManagementSystem.Infrastructure.Pricing;

public static class RegistrationExtensions
{
    public static IServiceCollection AddPricingServices(this IServiceCollection services)
    {
        services.AddOptions<VolumeDiscountOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection("VolumeDiscount").Bind(options));
        services.AddSingleton<IDiscountStrategy>(sp =>
            new VolumeDiscountStrategy(sp.GetRequiredService<IOptions<VolumeDiscountOptions>>().Value));
        services.AddSingleton<IDiscountStrategy, BlackFridayDiscountStrategy>();
        services.AddSingleton<IDiscountStrategy, HolidaySaleDiscountStrategy>();
        services.AddSingleton<IPricingCalculatorService, PricingCalculatorService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
