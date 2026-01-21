using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Infrastructure.Context;
using GenericRepository;

namespace ECommercePlatform.Infrastructure.Repositories;

internal sealed class CompanyInvitationRepository :
    Repository<CompanyInvitation, ApplicationDbContext>, ICompanyInvitationRepository
{
    public CompanyInvitationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
