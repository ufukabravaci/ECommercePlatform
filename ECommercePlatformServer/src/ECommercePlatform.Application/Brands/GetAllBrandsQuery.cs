using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Brands;



[Permission(PermissionConsts.ReadBrand)]
public sealed record GetAllBrandsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string Search = ""
) : IRequest<Result<PageResult<BrandDto>>>;

public sealed class GetAllBrandsQueryHandler(
    IRepository<Brand> brandRepository
) : IRequestHandler<GetAllBrandsQuery, Result<PageResult<BrandDto>>>
{
    public async Task<Result<PageResult<BrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = brandRepository.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.Name.Contains(request.Search));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<BrandDto>() // Mapster otomatik eşler
            .ToListAsync(cancellationToken);

        return new PageResult<BrandDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}