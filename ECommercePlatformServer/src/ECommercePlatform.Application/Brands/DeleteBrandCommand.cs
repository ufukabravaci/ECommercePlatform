using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Brands;

[Permission(PermissionConsts.DeleteBrand)]
public sealed record DeleteBrandCommand(Guid Id) : IRequest<Result<string>>;

public sealed class DeleteBrandCommandHandler(
    IRepository<Brand> brandRepository,
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteBrandCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByExpressionWithTrackingAsync(
            b => b.Id == request.Id,
            cancellationToken);

        if (brand is null)
            return Result<string>.Failure("Marka bulunamadı.");

        // İlişkili ürün kontrolü (Restrict Logic'i code-side yapıyoruz)
        bool hasProducts = await productRepository.AnyAsync(p => p.BrandId == request.Id, cancellationToken);
        if (hasProducts)
        {
            return Result<string>.Failure("Bu markaya ait ürünler bulunduğu için silinemez.");
        }

        // Soft Delete (Entity base method)
        brand.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Marka silindi.");
    }
}
