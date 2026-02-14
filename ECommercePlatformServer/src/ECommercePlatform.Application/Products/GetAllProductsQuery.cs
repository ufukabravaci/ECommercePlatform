using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

[Permission(PermissionConsts.ReadProduct)]
public sealed record GetAllProductsQuery(
    Guid? CategoryId = null,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PageResult<ProductDto>>>;


public sealed class GetAllProductsQueryHandler(
    IRepository<Product> productRepository
) : IRequestHandler<GetAllProductsQuery, Result<PageResult<ProductDto>>>
{
    public async Task<Result<PageResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // 1. changedetection kapalı
        // SELECT * FROM Products
        var query = productRepository.GetAll();

        // 2. Filtreleme dbde çalışır.
        // WHERE Name LIKE '%search%' OR Sku LIKE '%search%
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(request.Search) ||
                p.Sku.Contains(request.Search));
        }
        // CategoryId filtrelemesi
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }

        // 3. Toplam Kayıt Sayısını Al (Pagination Hesabı İçin)
        // bir int değer aldık ama productlar hala memoryde değil.
        int totalCount = await query.CountAsync(cancellationToken);

        // 4. SIRALAMA (pagination'dan önce)
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),

            "price" => request.SortDirection == "asc"
                ? query.OrderBy(p => p.Price.Amount)
                : query.OrderByDescending(p => p.Price.Amount),

            "createdat" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),

            // Default sıralama
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // 5. SAYFALAMA
        query = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        // Include kullanmaya gerek yok, EF Core aşağıdaki lambda'ya bakıp JOIN atacak.
        // Bu sayede sadece ihtiyacımız olan alanlar seçilmiş olur. TÜm category'i include etmemiş olduk.
        var products = await query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Description,
            p.Price.Amount,
            p.Price.Currency.ToString(),
            p.Stock,
            p.CategoryId,
            p.Category.Name,
            p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
            ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
            p.BrandId,
            p.Brand.Name,
            p.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsMain)).ToList()
        )).ToListAsync(cancellationToken);

        var result = new PageResult<ProductDto>
        {
            Items = products,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return result;
    }
}