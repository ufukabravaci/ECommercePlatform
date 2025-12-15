namespace ECommercePlatform.Application.Services;

public interface ITenantContext
{
    // Eğer null dönerse: Kullanıcı bir şirkete bağlı değildir (SuperAdmin vs.)
    Guid? GetCompanyId();
}
