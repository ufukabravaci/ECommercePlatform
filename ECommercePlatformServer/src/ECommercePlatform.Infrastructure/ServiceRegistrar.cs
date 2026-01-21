using ECommercePlatform.Application.Options;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.BackgroundJobs;
using ECommercePlatform.Infrastructure.Context;
using ECommercePlatform.Infrastructure.Options;
using ECommercePlatform.Infrastructure.Services;
using ECommercePlatform.Infrastructure.Tokens;
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
        services.ConfigureOptions<IdentitySetupOptions>();
        services.Configure<MailSettingOptions>(configuration.GetSection("MailSettings"));
        services.Configure<ClientSettings>(configuration.GetSection("ClientSettings"));
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
        services.AddScoped<IFileService, LocalFileService>();

        var redisCon = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisCon))
        {
            services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = redisCon;
                opt.InstanceName = "EComAPI_";
            });
        }
        services.AddScoped<IUnitOfWork>(srv => srv.GetRequiredService<ApplicationDbContext>());
        //scrutor ile otomatik servis kaydı
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ServiceRegistrar))
            .AddClasses(publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        //Identity config
        services.AddIdentityCore<User>()
       .AddRoles<AppRole>()
       .AddEntityFrameworkStores<ApplicationDbContext>()
       .AddDefaultTokenProviders()
       .AddTokenProvider<SixDigitTokenProvider<User>>("SixDigit"); //custom provider

        //Background services
        services.AddHostedService<TokenCleanupService>();
        return services;
    }
}
