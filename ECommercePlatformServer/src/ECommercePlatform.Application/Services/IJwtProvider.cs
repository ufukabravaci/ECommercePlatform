using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;

namespace ECommercePlatform.Application.Services;

public interface IJwtProvider
{
    Task<string> CreateTenantTokenAsync(
        User user,
        CompanyUser companyUser,
        CancellationToken cancellationToken);

    string CreateRefreshToken();
}
