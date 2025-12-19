using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class UserContext(
    IHttpContextAccessor _httpContextAccessor,
    RoleManager<AppRole> _roleManager,
    IDistributedCache _cache
    ) : IUserContext
{
    public Guid GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return Guid.Empty;
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim)) return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return false;

        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        if (!userRoles.Any()) return false;
        if (userRoles.Contains(RoleConsts.SuperAdmin)) return true;

        foreach (var roleName in userRoles)
        {
            // 1. Cache Key Oluştur (Örn: Role_CompanyOwner_Permissions)
            string cacheKey = $"Role_{roleName}_Permissions";

            // 2. Cache'e Bak
            List<string>? permissions;
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                // Cache'de var, deserialize et
                permissions = JsonSerializer.Deserialize<List<string>>(cachedData);
            }
            else
            {
                // 3. Cache'de yok, DB'ye git
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                // 4. Cache'e Yaz (Ömür: 1 Saat)
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(permissions), options);
            }

            // 5. Kontrol Et
            if (permissions != null && permissions.Contains(permissionCode))
            {
                return true;
            }
        }

        return false;
    }
}
