using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

public class UserRefreshTokenRepository : Repository<UserRefreshToken, ApplicationDbContext>, IUserRefreshTokenRepository
{
    public UserRefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
