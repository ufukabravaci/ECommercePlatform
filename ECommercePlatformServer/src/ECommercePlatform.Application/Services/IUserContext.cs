namespace ECommercePlatform.Application.Services;

public interface IUserContext
{
    Guid GetUserId();
    Task<bool> HasPermissionAsync(string permission);
}

