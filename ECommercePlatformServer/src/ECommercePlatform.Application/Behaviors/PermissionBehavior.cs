using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Exceptions;
using ECommercePlatform.Application.Services;
using System.Reflection;
using TS.MediatR;

namespace ECommercePlatform.Application.Behaviors;

public sealed class PermissionBehavior<TRequest, TResponse>(IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Request üzerindeki [Permission("...")] attribute'unu bul
        var permissionAttr = request.GetType().GetCustomAttribute<PermissionAttribute>();

        // Eğer attribute yoksa herkese açıktır, devam et.
        if (permissionAttr is null)
        {
            return await next();
        }

        // 2. Kullanıcıyı kontrol et
        var userId = userContext.GetUserId();
        if (userId == Guid.Empty)
        {
            // Giriş yapmamış ama yetki isteyen biri
            throw new UnauthorizedAccessException("Bu işlemi yapmak için giriş yapmalısınız.");
        }

        // 3. Yetkiyi kontrol et
        if (!await userContext.HasPermissionAsync(permissionAttr.Permission))
        {
            throw new ForbiddenAccessException(
                $"'{permissionAttr.Permission}' yetkiniz yok.");
        }

        return await next();
    }
}