using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null
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

        // 3. Toplam Kayıt Sayısını Al (Pagination Hesabı İçin)
        // DİKKAT: Bunu Paging (Skip/Take) yapmadan ÖNCE almalıyız.
        // bir int değer aldık ama productlar hala memoryde değil.
        int totalCount = await query.CountAsync(cancellationToken);

        // 4. Projection (SQL SELECT Optimizasyonu)
        // Include kullanmaya gerek yok, EF Core aşağıdaki lambda'ya bakıp JOIN atacak.
        // Bu sayede sadece ihtiyacımız olan alanlar seçilmiş olur. TÜm category'i include etmemiş olduk.
        var dtoQuery = query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Description,
            p.Price.Amount,               // Value Object'ten property okuma
            p.Price.Currency.ToString(),  // Enum to String
            p.Stock,
            p.CategoryId,
            p.Category.Name,              // EF Core burada otomatik JOIN atar

            // Ana resmi bulma mantığı (SQL'e çevrilir)
            p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
            ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),

            // Alt resim listesi
            p.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsMain)).ToList()
        ));

        // 5. Sıralama ve Sayfalama (SQL'de çalışır)
        var products = await dtoQuery
            .OrderByDescending(p => p.Name) // Sıralama kriteri eklenebilir
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 6. Sonuç
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