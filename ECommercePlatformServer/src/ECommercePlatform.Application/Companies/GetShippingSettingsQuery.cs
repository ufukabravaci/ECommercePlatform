using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;

public sealed record ShippingSettingsDto(decimal FreeShippingThreshold, decimal FlatRate);

// Auth zorunlu değil ama TenantContext (Header) zorunlu.
public sealed record GetShippingSettingsQuery() : IRequest<Result<ShippingSettingsDto>>;

public sealed class GetShippingSettingsQueryHandler(
    IRepository<Company> companyRepository,
    ITenantContext tenantContext
) : IRequestHandler<GetShippingSettingsQuery, Result<ShippingSettingsDto>>
{
    public async Task<Result<ShippingSettingsDto>> Handle(GetShippingSettingsQuery request, CancellationToken cancellationToken)
    {
        // Projection (Sadece ayarları çekiyoruz, tüm şirketi değil)
        var settings = await companyRepository.AsQueryable()
            .Where(c => c.Id == tenantContext.CompanyId)
            .Select(c => new ShippingSettingsDto(
                c.ShippingSettings.FreeShippingThreshold,
                c.ShippingSettings.FlatRate))
            .FirstOrDefaultAsync(cancellationToken);

        // Eğer henüz ayar yapılmadıysa varsayılan dön
        if (settings is null)
            return new ShippingSettingsDto(0, 0);

        return settings;
    }
}
