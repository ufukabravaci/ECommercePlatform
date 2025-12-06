using ECommercePlatform.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return Guid.Empty;
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim)) return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public Task<bool> HasPermissionAsync(string permissionCode)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return Task.FromResult(false);
        // Claim("Permission", "Users.Create")
        var hasPermission = context.User?.HasClaim(c => c.Type == "Permission" && c.Value == permissionCode);

        return Task.FromResult(hasPermission ?? false);
    }
}