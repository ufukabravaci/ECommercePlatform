using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

public sealed record RemoveProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : IRequest<Result<string>>;

public sealed class RemoveProductImageCommandHandler(
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService
) : IRequestHandler<RemoveProductImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        // 1. Ürünü Resimlerle Getir
        var product = await productRepository
            .WhereWithTracking(p => p.Id == request.ProductId)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return Result<string>.Failure("Ürün bulunamadı.");

        // 2. Silinecek resmin URL'ini bul (Diskten silmek için lazım)
        // Domain encapsulation olduğu için linq ile buluyoruz
        var imageToRemove = product.Images.FirstOrDefault(x => x.Id == request.ImageId);

        if (imageToRemove is null)
            return Result<string>.Failure("Silinecek resim bulunamadı.");

        string pathToDelete = imageToRemove.ImageUrl;

        // 3. Domain Metodunu Çağır (Koleksiyondan çıkarır, Main'i ayarlar)
        try
        {
            product.RemoveImage(request.ImageId);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        // 4. DB'yi Güncelle
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Dosyayı Diskten Sil (Sadece DB başarılı olursa buraya gelir)
        fileService.Delete(pathToDelete);

        return "Resim başarıyla silindi.";
    }
}