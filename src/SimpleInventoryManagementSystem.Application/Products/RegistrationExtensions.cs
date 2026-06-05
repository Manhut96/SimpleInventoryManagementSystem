using Microsoft.Extensions.DependencyInjection;

namespace SimpleInventoryManagementSystem.Application.Products;

public static class RegistrationExtensions
{
    public static IServiceCollection AddProductsFeature(this IServiceCollection services)
    {
        return services;
    }
}
