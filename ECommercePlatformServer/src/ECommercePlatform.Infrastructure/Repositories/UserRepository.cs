using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User, ApplicationDbContext>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
