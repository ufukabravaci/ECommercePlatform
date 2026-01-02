using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

[Permission(PermissionConsts.UpdateProduct)]
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Stock,
    Guid CategoryId
) : IRequest<Result<string>>;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ürün ID'si boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama 2000 karakterden fazla olamaz.");

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0).WithMessage("Ürün fiyatı 0'dan büyük olmalıdır.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Lütfen bir kategori seçiniz.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi boş olamaz.")
            .Must(c => Enum.TryParse<Currency>(c, true, out _))
            .WithMessage("Geçersiz para birimi seçimi.");
    }
}

public sealed class UpdateProductCommandHandler(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProductCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Entity'i Tracking Modunda Getir
        // Global Filter devreye girer, sadece kendi şirketinin ürününü bulabilir.
        var product = await productRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == request.Id,
            cancellationToken);

        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı veya bu işlem için yetkiniz yok.");
        }

        // 2. Kategori Değişikliği ve Güvenlik Kontrolü
        if (product.CategoryId != request.CategoryId)
        {
            // Seçilen yeni kategorinin bu şirkete (Tenant) ait olup olmadığını doğrula
            bool isCategoryValid = await categoryRepository.AnyAsync(
                c => c.Id == request.CategoryId && c.CompanyId == product.CompanyId,
                cancellationToken);

            if (!isCategoryValid)
            {
                return Result<string>.Failure("Seçilen kategori geçersiz veya şirketinize ait değil.");
            }

            product.SetCategory(request.CategoryId);
        }

        // 2. Domain Metotları ile Güncelleme
        // Bu metotlar kendi içlerinde validasyon yapar (Domain Guards)
        product.SetName(request.Name);
        product.SetDescription(request.Description);
        product.UpdateStock(request.Stock);

        // 3. Domain Metotları ile State Update
        // Domain katmanında yazdığımız Guard Clause'lar (if throw...) burada otomatik çalışır.
        product.SetName(request.Name);
        product.SetDescription(request.Description);
        product.UpdateStock(request.Stock);

        // Fiyat ve Para Birimi Kontrolü (Value Object - Money)
        var newCurrency = Enum.Parse<Currency>(request.CurrencyCode, true);
        var newPrice = new Money(request.PriceAmount, newCurrency);

        // Money bir record veya Equals override edilmiş bir class olduğu için direkt karşılaştırılabilir
        if (!product.Price.Equals(newPrice))
        {
            product.SetPrice(newPrice);
        }

        // 4. Kalıcılık (Persistence)
        // EF Core Tracking sayesinde sadece değişen kolonlar SQL'e yansıtılır.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Ürün bilgileri başarıyla güncellendi.");
    }
}