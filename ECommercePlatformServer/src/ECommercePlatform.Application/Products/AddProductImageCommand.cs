using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Products;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

public sealed record AddProductImageCommand(
    Guid ProductId,
    IFormFile File,
    bool IsMain
) : IRequest<Result<string>>;

public sealed class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.File).NotNull().WithMessage("Dosya seçilmelidir.");

        RuleFor(x => x.File)
            .Must(f => f.Length <= 5 * 1024 * 1024)
            .WithMessage("Dosya boyutu 5MB'dan büyük olamaz.")
            .Must(f => f.ContentType.StartsWith("image/"))
            .WithMessage("Sadece resim dosyası yüklenebilir.")
            .Must(f => _allowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage("Geçersiz dosya uzantısı.");
    }
}

public sealed class AddProductImageCommandHandler(
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    ITenantContext tenantContext
) : IRequestHandler<AddProductImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        // 1. Ürünü Resimlerle Birlikte Getir (Domain kuralı: Max 5 resim kontrolü için)
        // Global Query Filter (Tenant) burada da aktiftir.
        var product = await productRepository
            .WhereWithTracking(p => p.Id == request.ProductId)
            .Include(p => p.Images) // Domain logic count kontrolü yapacak
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return Result<string>.Failure("Ürün bulunamadı.");

        string? uploadedPath = null;

        try
        {
            // 2. Dosyayı Yükle
            using var stream = request.File.OpenReadStream();

            uploadedPath = await fileService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                $"{tenantContext.CompanyId}/products",
                cancellationToken);

            // 3. Domain Metodunu Çağır (Max 5 kontrolü burada yapılır)
            product.AddImage(uploadedPath, request.IsMain);

            // 4. Kaydet
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return "Resim başarıyla eklendi.";
        }
        catch (Exception ex)
        {
            // Rollback: Hata olursa yüklenen dosyayı sil
            if (uploadedPath is not null)
            {
                fileService.Delete(uploadedPath);
            }

            // Domain exceptionlarını (örn: "En fazla 5 resim olabilir") yakala ve kullanıcıya dön
            if (ex is InvalidOperationException || ex is ArgumentException)
                return Result<string>.Failure(ex.Message);

            throw; // Beklenmeyen hataları fırlat
        }
    }
}