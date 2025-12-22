using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Exceptions;
using ECommercePlatform.Application.Services;
using System.Reflection;
using TS.MediatR;

public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext,
    ITenantContext tenantContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var permissionAttr = request
            .GetType()
            .GetCustomAttribute<PermissionAttribute>();

        // Permission yoksa serbest
        if (permissionAttr is null)
            return await next();

        var userId = userContext.GetUserId();
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Giriş yapmalısınız.");

        // 3. Tenant (Şirket) Context'i var mı?
        // Header'dan (X-Tenant-ID) veya Token'dan (CompanyId claim) gelmeli.
        if (tenantContext.CompanyId is null)
        {
            // Kullanıcı login, ama hangi şirkette işlem yaptığını bilmiyoruz.
            throw new ForbiddenAccessException("İşlem yapmak için bir şirket bağlamı (Tenant) gereklidir.");
        }

        // 4. Yetki Kontrolü
        // UserContext, token içindeki rolleri okur. Token üretilirken o şirkete ait roller gömüldü.
        if (!await userContext.HasPermissionAsync(permissionAttr.Permission))
        {
            throw new ForbiddenAccessException("Bu işlem için yetkiniz bulunmamaktadır.");
        }

        return await next();
    }
}