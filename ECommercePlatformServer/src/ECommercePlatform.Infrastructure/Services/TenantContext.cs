using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class TenantContext(IHttpContextAccessor _httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? CompanyId
    {
        get
        {
            // 1. Token'da var mı?
            var claimValue = User?.FindFirst(ClaimTypesConst.CompanyId)?.Value;
            if (Guid.TryParse(claimValue, out var tokenId)) return tokenId;

            // 2. Header'da var mı?
            var headerValue = _httpContextAccessor.HttpContext?
                .Request.Headers[ClaimTypesConst.TenantIdHeader]
                .FirstOrDefault();

            if (Guid.TryParse(headerValue, out var headerId)) return headerId;

            return null;
        }
    }
}