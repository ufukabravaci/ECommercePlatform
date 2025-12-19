using ECommercePlatform.Application.Behaviors;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace ECommercePlatform.Application;

public static class ServiceRegistrar
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfr =>
        {
            cfr.RegisterServicesFromAssembly(typeof(ServiceRegistrar).Assembly);
            cfr.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfr.AddOpenBehavior(typeof(PermissionBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(ServiceRegistrar).Assembly);
        services.AddMapster();
        return services;
    }
}
