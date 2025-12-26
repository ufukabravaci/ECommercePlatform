using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using MapsterMapper;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;


[Permission(PermissionConsts.ReadCompany)]
public sealed record GetCompanyQuery() : IRequest<Result<CompanyDto>>;

public sealed class GetCompanyQueryHandler(
    ICompanyRepository companyRepository,
    ITenantContext tenantContext,
    IMapper mapper
    ) : IRequestHandler<GetCompanyQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        // 1. Token'dan Şirket ID'sini al
        var companyId = tenantContext.CompanyId;
        if (companyId is null) return Result<CompanyDto>.Failure("Herhangi bir şirkete bağlı değilsiniz.");

        // 2. Veriyi çek
        var company = await companyRepository.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken, false);

        if (company is null) return Result<CompanyDto>.Failure("Şirket bilgisi bulunamadı.");

        var dto = mapper.Map<CompanyDto>(company);

        return dto;
    }
}
