using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace ECommercePlatform.Infrastructure;

public static class ServiceRegistrar
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            string con = configuration.GetConnectionString("SqlServer")!;
            opt.UseSqlServer(con);
        });
        services.AddScoped<IUnitOfWork>(srv => srv.GetRequiredService<ApplicationDbContext>());
        //scrutor ile otomatik servis kaydı
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ServiceRegistrar))
            .AddClasses(publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        //Identity config
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
        // 5. Generic Repository & UnitOfWork
        // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        // services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
