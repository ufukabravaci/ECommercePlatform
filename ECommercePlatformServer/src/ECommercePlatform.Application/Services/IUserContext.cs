namespace ECommercePlatform.Application.Services;

public interface IUserContext
{
    Guid GetUserId();
    //JWT Claimsten okunacak.
    Task<bool> HasPermissionAsync(string permissionCode);
}
