using ECommercePlatform.Domain.Banners;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class BannerRepository : Repository<Banner, ApplicationDbContext>, IBannerRepository
{
    public BannerRepository(ApplicationDbContext context) : base(context)
    {
    }
}
