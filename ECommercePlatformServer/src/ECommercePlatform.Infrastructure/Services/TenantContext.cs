using ECommercePlatform.Application.Services;
using Microsoft.AspNetCore.Http;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCompanyId()
    {
        // HttpContext yoksa (örneğin Background Job çalışıyorsa) null dön
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return null;

        // Token içindeki "CompanyId" claim'ini oku
        var companyIdClaim = context.User?.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim)) return null;

        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }
}
