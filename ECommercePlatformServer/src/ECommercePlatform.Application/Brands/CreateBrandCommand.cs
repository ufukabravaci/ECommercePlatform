using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Brands;

[Permission(PermissionConsts.CreateBrand)]
public sealed record CreateBrandCommand(
    string Name,
    string? LogoUrl
) : IRequest<Result<string>>;

public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
    }
}

public sealed class CreateBrandCommandHandler(
    IRepository<Brand> brandRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext
) : IRequestHandler<CreateBrandCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        // Aynı isimde marka var mı? (Global Filter CompanyId'yi zaten süzer)
        bool isExists = await brandRepository.AnyAsync(
            b => b.Name == request.Name,
            cancellationToken);

        if (isExists)
            return Result<string>.Failure("Bu isimde bir marka zaten mevcut.");

        var brand = new Brand(request.Name, request.LogoUrl, tenantContext.CompanyId!.Value);

        brandRepository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Marka başarıyla oluşturuldu.");
    }
}