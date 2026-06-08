using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SimpleInventoryManagementSystem.Application.Common.Behaviors;

namespace SimpleInventoryManagementSystem.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAssemblyMarker).Assembly));
        services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
