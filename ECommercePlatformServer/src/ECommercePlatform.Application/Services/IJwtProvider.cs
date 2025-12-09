using ECommercePlatform.Domain.Users;

namespace ECommercePlatform.Application.Services;

public interface IJwtProvider
{
    Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
    string GenerateRefreshToken();
}
