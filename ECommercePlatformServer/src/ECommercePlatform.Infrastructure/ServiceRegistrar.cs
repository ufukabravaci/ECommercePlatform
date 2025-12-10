using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.BackgroundJobs;
using ECommercePlatform.Infrastructure.Context;
using ECommercePlatform.Infrastructure.Options;
using GenericRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.ConfigureOptions<JwtSetupOptions>();
        services.Configure<MailSettingOptions>(configuration.GetSection("MailSettings"));
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();
        services.AddAuthorization();

        var mailSettings = configuration.GetSection("MailSettings").Get<MailSettingOptions>();
        if (mailSettings is not null)
        {
            var fluentEmail = services.AddFluentEmail(mailSettings.SenderEmail);

            if (string.IsNullOrEmpty(mailSettings.UserId))
            {
                fluentEmail.AddSmtpSender(mailSettings.Smtp, mailSettings.Port);
            }
            else
            {
                fluentEmail.AddSmtpSender(mailSettings.Smtp, mailSettings.Port, mailSettings.UserId, mailSettings.Password);
            }
        }
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
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        services.AddHostedService<TokenCleanupService>();
        return services;
    }
}
