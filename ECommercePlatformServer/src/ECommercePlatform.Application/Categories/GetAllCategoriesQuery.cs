using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Categories;

[Permission(PermissionConsts.ReadCategory)]
public sealed record GetAllCategoriesQuery() : IRequest<Result<List<CategoryDto>>>;

// DTO
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string ParentName
);

public sealed class GetAllCategoriesQueryHandler(
    IRepository<Category> categoryRepository
) : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryDto>>>
{
    public async Task<Result<List<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAll() // IQueryable
            .OrderBy(x => x.Name)
            .ProjectToType<CategoryDto>() // Mapster Projection
            .ToListAsync(cancellationToken);

        return categories;
    }
}
