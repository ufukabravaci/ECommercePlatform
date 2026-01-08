using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Brands;

[Permission(PermissionConsts.UpdateBrand)]
public sealed record UpdateBrandCommand(
    Guid Id,
    string Name,
    string? LogoUrl
) : IRequest<Result<string>>;

public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
    }
}

public sealed class UpdateBrandCommandHandler(
    IRepository<Brand> brandRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateBrandCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByExpressionWithTrackingAsync(
            b => b.Id == request.Id,
            cancellationToken);

        if (brand is null)
            return Result<string>.Failure("Marka bulunamadı.");

        // İsim değişiyorsa duplicate kontrolü
        if (brand.Name != request.Name)
        {
            bool isExists = await brandRepository.AnyAsync(b => b.Name == request.Name, cancellationToken);
            if (isExists) return Result<string>.Failure("Bu isimde başka bir marka zaten var.");
        }

        brand.Update(request.Name, request.LogoUrl);

        // Repostory.Update(brand) GenericRepository'de tracking açıksa şart değil ama 
        // convention olarak koyabilirsin.

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Marka güncellendi.");
    }
}