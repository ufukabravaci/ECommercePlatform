using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Infrastructure.Options;

public sealed class IdentitySetupOptions : IConfigureOptions<IdentityOptions>
{
    public void Configure(IdentityOptions options)
    {
        // 1. Şifre Kuralları
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        // 2. Kullanıcı Kuralları
        options.User.RequireUniqueEmail = true;

        // 3. Lockout (Kitleme) Kuralları
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // 4. Token Provider Ayarları
        // Email Onayı için bizim yazdığımız "SixDigit" provider'ı kullan.
        options.Tokens.ProviderMap["SixDigit"] =
            new TokenProviderDescriptor(typeof(SixDigitTokenProvider<User>));
        options.Tokens.EmailConfirmationTokenProvider = "SixDigit";
        options.Tokens.ChangeEmailTokenProvider = "SixDigit";
        options.Tokens.PasswordResetTokenProvider = "SixDigit";

    }
}