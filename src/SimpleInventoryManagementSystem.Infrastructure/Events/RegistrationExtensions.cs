using Microsoft.Extensions.DependencyInjection;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Infrastructure.BackgroundServices;
using SimpleInventoryManagementSystem.Infrastructure.Services;

namespace SimpleInventoryManagementSystem.Infrastructure.Events;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEventServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, MockEventPublisher>();
        services.AddHostedService<OutboxProcessor>();
        return services;
    }
}
