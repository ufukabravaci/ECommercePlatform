using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal class UserRefreshTokenRepository : Repository<UserRefreshToken, ApplicationDbContext>, IUserRefreshTokenRepository
{
    public UserRefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
