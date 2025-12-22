using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class CategoryRepository : Repository<Category, ApplicationDbContext>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {

    }
}
