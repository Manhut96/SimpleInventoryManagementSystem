using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Infrastructure.Services;

namespace SimpleInventoryManagementSystem.Infrastructure.Persistence;

public static class RegistrationExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SIMSDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISIMSDbContext>(provider => provider.GetRequiredService<SIMSDbContext>());
        services.AddScoped<ITransactionScopeFactory, EfTransactionScopeFactory>();

        return services;
    }
}
