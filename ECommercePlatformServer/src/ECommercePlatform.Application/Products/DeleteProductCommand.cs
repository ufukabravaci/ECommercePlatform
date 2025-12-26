using ECommercePlatform.Domain.Products;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;


//soft delete olduğu için ürün resimlerini silmiyorum. Bu davranış değiştirilebilir.
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
        // Generic Repo'nun Delete metodu Entity.Remove çağırır.
        // Infrastructure katmanında SaveChanges override edilmişse bu işlem Soft Delete'e döner.
        // Edilmemişse DB'den siler. Proje kurallarında Soft Delete olduğu belirtilmişti.
        productRepository.Delete(product);

        // 3. Kayıt
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Ürün başarıyla silindi.";
    }
}