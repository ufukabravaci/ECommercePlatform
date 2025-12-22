using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class CompanyUserRepository : Repository<CompanyUser, ApplicationDbContext>, ICompanyUserRepository
{
    public CompanyUserRepository(ApplicationDbContext context) : base(context)
    {
    }
}