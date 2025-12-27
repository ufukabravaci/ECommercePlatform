using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Products;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ürün ID'si boş olamaz.")
            .NotEqual(Guid.Empty).WithMessage("Geçersiz ürün kimliği.");
    }
}

public sealed class GetProductByIdQueryHandler(
    IRepository<Product> productRepository
) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Sorguyu hazırla (Tracking kapalı - AsNoTracking)
        var query = productRepository.Where(p => p.Id == request.Id);

        // 2. Projeksiyon (Sadece ihtiyacımız olan alanları SQL'den çekiyoruz)
        var productDto = await query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Description,
            p.Price.Amount,
            p.Price.Currency.ToString(),
            p.Stock,
            p.CategoryId,
            p.Category.Name,

            // Ana resim önceliği: IsMain olan, yoksa ilk resim, o da yoksa null
            p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
            ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),

            // Tüm resim listesi
            p.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsMain)).ToList()
        )).FirstOrDefaultAsync(cancellationToken);

        // 3. Kontrol
        if (productDto is null)
        {
            return Result<ProductDto>.Failure("Ürün bulunamadı.");
        }

        return Result<ProductDto>.Succeed(productDto);
    }
}