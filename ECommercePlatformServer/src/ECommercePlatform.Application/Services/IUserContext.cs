namespace ECommercePlatform.Application.Services;

public interface IUserContext
{
    Guid GetUserId();
    // Bu metod kullanıcının rollerine bakacak, o rollerin claimlerini (permissionlarını) bulacak
    // ve istenen permission var mı diye dönecek.
    Task<bool> HasPermissionAsync(string permissionCode);
}
