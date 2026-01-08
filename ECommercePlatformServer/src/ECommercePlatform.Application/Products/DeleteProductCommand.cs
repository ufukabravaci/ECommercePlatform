using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;


[Permission(PermissionConsts.ManageProductImages)]
public sealed record DeleteProductCommand(Guid Id) : IRequest<Result<string>>;

public sealed class DeleteProductCommandHandler(
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteProductCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Entity'i Getir (Tracking önemli, çünkü state değişecek)
        var product = await productRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == request.Id,
            cancellationToken);

        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı.");
        }

        // 2. Silme İşlemi
        // override edilmiş Delete metodu kullanılıyor
        product.Delete();

        // 3. Kayıt
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Ürün başarıyla silindi.";
    }
}