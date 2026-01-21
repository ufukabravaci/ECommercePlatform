using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.Json;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class UserContext(
    IHttpContextAccessor _httpContextAccessor,
    IServiceProvider _serviceProvider,
    IDistributedCache _cache
    ) : IUserContext
{
    public Guid GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return Guid.Empty;
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User?.FindFirst("sub")?.Value; ;

        if (string.IsNullOrEmpty(userIdClaim)) return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return false;

        // 1. Token'dan Rolleri Oku
        // (JwtProvider bu rolleri CompanyUser tablosundan alıp token'a koymuştu)
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        if (!userRoles.Any()) return false;

        // SuperAdmin her şeye yetkilidir
        if (userRoles.Contains(RoleConsts.SuperAdmin)) return true;

        // 1. ÖNCE TOKEN CLAIM KONTROLÜ (Kişiye Özel Yetkiler Burada)
        // Eğer JwtProvider ile kişiye özel yetkiyi token'a bastıysak, burada direkt yakalarız.
        // Veritabanı veya Cache'e gitmeye gerek kalmaz.
        var hasDirectPermission = context.User.Claims.Any(c =>
            c.Type == ClaimTypesConst.Permission &&
            c.Value == permissionCode);

        if (hasDirectPermission) return true;

        // Lazy Loading ile RoleManager'ı al (Circular Dependency önlemek için)
        // ctordan alsaydık uygulama ayağa kalkarken döngüye girerdi.Onun yerine servis kutusu istedik onun içinden manuel çektik.
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<AppRole>>();

        foreach (var roleName in userRoles)
        {
            // 2. Cache Key Oluştur
            string cacheKey = $"Role_{roleName}_Permissions";

            // 3. Cache Kontrol
            List<string>? permissions;
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                permissions = JsonSerializer.Deserialize<List<string>>(cachedData);
            }
            else
            {
                // 4. DB'den Yetkileri Çek (AspNetRoleClaims)
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await roleManager.GetClaimsAsync(role);
                permissions = claims.Where(c => c.Type == ClaimTypesConst.Permission).Select(c => c.Value).ToList();

                // 5. Cache'e Yaz (1 Saat)
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(permissions), options);
            }

            // 6. Yetki Var mı?
            if (permissions != null && permissions.Contains(permissionCode))
            {
                return true;
            }
        }

        return false;
    }
}