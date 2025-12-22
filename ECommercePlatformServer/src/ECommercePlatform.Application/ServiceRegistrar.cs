using ECommercePlatform.Application.Behaviors;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
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
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
