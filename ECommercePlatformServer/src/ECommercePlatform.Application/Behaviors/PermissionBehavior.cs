using ECommercePlatform.Application.Services;
using System.Reflection;
using TS.MediatR;

namespace ECommercePlatform.Application.Behaviors;

// 1. BEHAVIOR (Davranış)
public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. İsteğin (Command/Query) üzerindeki Permission attribute'ünü bul
        var permissionAttr = request.GetType().GetCustomAttribute<PermissionAttribute>(inherit: true);

        // Eğer attribute yoksa, yetki kontrolüne gerek yoktur. Devam et.
        if (permissionAttr is null)
            return await next();

        // 2. Kullanıcı giriş yapmış mı? (Authentication Kontrolü)
        var userId = userContext.GetUserId();
        if (userId == Guid.Empty) // Veya null kontrolü
        {
            throw new AuthorizationException("Bu işlemi yapmak için giriş yapmalısınız.");
        }

        // 3. Kullanıcının gerekli yetkisi var mı? (Authorization Kontrolü)
        // Not: Permission string'i boş ise sadece giriş yapmış olması yeterlidir mantığı güdüldü.
        if (!string.IsNullOrEmpty(permissionAttr.Permission))
        {
            // Veritabanına gitmek yerine, UserContext üzerinden (genelde Memory veya Token'dan) kontrol ediyoruz.
            bool hasPermission = await userContext.HasPermissionAsync(permissionAttr.Permission);

            if (!hasPermission)
            {
                throw new AuthorizationException($"Bu işlem için yetkiniz bulunmamaktadır.");
            }
        }

        // Her şey yolunda, Handler'a geç.
        return await next();
    }
}

// 2. ATTRIBUTE (Etiket)
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PermissionAttribute : Attribute
{
    public string? Permission { get; }

    // Kullanım: [Permission("Users.Create")]
    public PermissionAttribute(string permission)
    {
        Permission = permission;
    }

    // Kullanım: [Permission] -> Sadece giriş yapmış olması yeterli
    public PermissionAttribute()
    {
        Permission = null;
    }
}

// 3. EXCEPTION (Hata Sınıfı)
public sealed class AuthorizationException : Exception
{
    public AuthorizationException(string message) : base(message)
    {
    }
}