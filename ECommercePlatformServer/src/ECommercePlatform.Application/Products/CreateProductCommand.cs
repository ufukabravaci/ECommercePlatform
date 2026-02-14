using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

[Permission(PermissionConsts.CreateProduct)]
public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Stock,
    Guid BrandId,
    Guid CategoryId,
    IFormFileCollection? Files // <--- Native koleksiyon tipi
) : IRequest<Result<Guid>>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Ürün adı 200 karakteri geçemez.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Stok kodu (SKU) boş olamaz.")
            .MinimumLength(3).WithMessage("SKU en az 3 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("SKU 50 karakteri geçemez.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama 2000 karakteri geçemez.");

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Marka seçimi zorunludur.")
            .NotEqual(Guid.Empty).WithMessage("Geçersiz marka.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur.")
            .NotEqual(Guid.Empty).WithMessage("Geçersiz kategori.");

        // Currency Kontrolü: Gelen string (Örn: "TRY"), bizim Enum'da var mı?
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi boş olamaz.")
            .Must(beAValidCurrency).WithMessage($"Geçersiz para birimi. Geçerli değerler: {string.Join(", ", Enum.GetNames(typeof(Currency)))}");

        RuleFor(x => x.Files)
            .Must(files => files == null || files.Count <= 20)
            .WithMessage("En fazla 20 resim yükleyebilirsiniz.");

        RuleForEach(x => x.Files).ChildRules(file =>
        {
            // 1. Boyut Kontrolü (Max 5MB)
            file.RuleFor(f => f.Length)
                .GreaterThan(0).WithMessage("Dosya boş olamaz.")
                .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("Dosya boyutu 5MB'dan büyük olamaz.");

            // 2. Content Type (Header) Kontrolü
            file.RuleFor(f => f.ContentType)
                .Must(ct => ct.StartsWith("image/"))
                .WithMessage("Dosya tipi geçersiz. Sadece resim yüklenebilir.");

            // 3. Uzantı (Extension) Kontrolü - YENİ EKLENEN KISIM
            file.RuleFor(f => f.FileName)
                .Must(fileName =>
                {
                    var extension = Path.GetExtension(fileName).ToLowerInvariant();
                    return _allowedExtensions.Contains(extension);
                })
                .WithMessage($"Geçersiz dosya uzantısı. İzin verilenler: {string.Join(", ", _allowedExtensions)}");
        });
    }

    private bool beAValidCurrency(string currencyCode)
    {
        return Enum.TryParse<Currency>(currencyCode, true, out _);
    }
}


public sealed class CreateProductCommandHandler(
    IRepository<Product> productRepository,
    IRepository<Brand> brandRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService, // Infrastructure'dan gelen servis
    ITenantContext tenantContext
) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Validasyonlar & Tenant
        Guid? companyId = tenantContext.CompanyId;
        if (!companyId.HasValue) return Result<Guid>.Failure("Şirket bilgisi yok.");

        // SKU Unique Kontrolü (DB)
        bool isSkuExists = await productRepository.AnyAsync(
            p => p.Sku == request.Sku && p.CompanyId == companyId.Value,
            cancellationToken);
        if (isSkuExists) return Result<Guid>.Failure("Bu SKU zaten kullanımda.");

        // MARKANIN GEÇERLİLİĞİNİ KONTROL ET
        bool isBrandValid = await brandRepository.AnyAsync(
            b => b.Id == request.BrandId && b.CompanyId == tenantContext.CompanyId,
            cancellationToken);

        if (!isBrandValid)
            return Result<Guid>.Failure("Geçersiz marka seçimi.");

        // 2. Entity Oluşturma
        var currency = Enum.Parse<Currency>(request.CurrencyCode);
        var price = new Money(request.PriceAmount, currency);

        var product = new Product(
            request.Name,
            request.Sku,
            request.Description,
            price,
            request.Stock,
            companyId.Value,
            request.BrandId,
            request.CategoryId
        );

        // 3. DOSYA YÜKLEME VE ROLLBACK MANTIĞI
        List<string> uploadedPaths = new();

        try
        {
            if (request.Files is not null)
            {
                foreach (var file in request.Files)
                {
                    // KRİTİK NOKTA: Stream'i burada açıyoruz.
                    // 'using' bloğu bittiği an stream kapanır (Dispose).
                    // ASP.NET Core geçici dosyayı request bitince temizler.
                    using var stream = file.OpenReadStream();

                    string path = await fileService.UploadAsync(
                        stream,
                        file.FileName,
                        file.ContentType,
                        $"{companyId}/products",
                        cancellationToken);

                    uploadedPaths.Add(path);
                    product.AddImage(path);
                }
            }

            await productRepository.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Succeed(product.Id);
        }
        catch (Exception)
        {
            // Rollback: Hata olursa yüklenenleri sil
            foreach (var path in uploadedPaths)
            {
                try { fileService.Delete(path); } catch { }
            }
            throw;
        }
    }
}