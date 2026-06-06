using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInventoryManagementSystem.Infrastructure.Events;
using SimpleInventoryManagementSystem.Infrastructure.Persistence;
using SimpleInventoryManagementSystem.Infrastructure.Pricing;

namespace SimpleInventoryManagementSystem.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddPricingServices();
        services.AddEventServices();
        return services;
    }
}
