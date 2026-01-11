using ECommercePlatform.Domain.Banners;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Banners;

public sealed record BannerDto(
    Guid Id,
    string Title,
    string Description,
    string ImageUrl,
    string TargetUrl,
    int Order);

// Bu endpoint Public de olabilir (Anasayfa için) veya Auth gerektirebilir.
// Yönetim paneli için Auth şart, Storefront için şart değil.
public sealed record GetAllBannersQuery() : IRequest<Result<List<BannerDto>>>;

public sealed class GetAllBannersQueryHandler(
    IRepository<Banner> bannerRepository
) : IRequestHandler<GetAllBannersQuery, Result<List<BannerDto>>>
{
    public async Task<Result<List<BannerDto>>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        // Sıralı getir
        var banners = await bannerRepository.AsQueryable()
            .OrderBy(x => x.Order)
            .ProjectToType<BannerDto>()
            .ToListAsync(cancellationToken);

        return banners;
    }
}
