using Microsoft.Extensions.DependencyInjection;

namespace SimpleInventoryManagementSystem.Application.Orders;

public static class RegistrationExtensions
{
    public static IServiceCollection AddOrdersFeature(this IServiceCollection services)
    {
        return services;
    }
}
