using ECommercePlatform.Domain.Reviews;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class ReviewRepository : Repository<Review, ApplicationDbContext>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }
}
