using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Products;

[Permission(PermissionConsts.ManageProductImages)]
public sealed record SetMainProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : IRequest<Result<string>>;

public sealed class SetMainProductImageCommandHandler(
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetMainProductImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        // 1. Ürünü ve Resimlerini Getir
        var product = await productRepository
            .WhereWithTracking(p => p.Id == request.ProductId)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return Result<string>.Failure("Ürün bulunamadı.");

        // 2. Domain Metodu
        try
        {
            product.SetMainImage(request.ImageId);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        // 3. Kaydet
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Ana resim güncellendi.";
    }
}