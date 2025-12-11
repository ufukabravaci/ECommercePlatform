using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Infrastructure.Tokens;

// Sayısal kod üretmesi için TotpSecurityStampBasedTokenProvider'dan türetiyoruz.
public class SixDigitTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : class
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(true);
    }

    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        // Token'ın sadece o kullanıcıya ve o amaca özel olması için bir imza
        var email = await manager.GetEmailAsync(user);
        return "EmailCode:" + purpose + ":" + email;
    }
}