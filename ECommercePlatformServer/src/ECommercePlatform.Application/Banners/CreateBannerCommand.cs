using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Banners;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Banners;

[Permission(PermissionConsts.CreateBanner)]
public sealed record CreateBannerCommand(
    string Title,
    string Description,
    IFormFile Image,
    string TargetUrl,
    int Order
) : IRequest<Result<Guid>>;

public sealed class CreateBannerCommandValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Image).NotNull().WithMessage("Banner görseli zorunludur.");
    }
}

public sealed class CreateBannerCommandHandler(
    IRepository<Banner> bannerRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IFileService fileService
) : IRequestHandler<CreateBannerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        Guid companyId = tenantContext.CompanyId!.Value;

        // 1. Dosya Yükleme
        // Klasör yapısı: uploads/banners/{companyId}/
        string folderName = $"banners/{companyId}";

        using var stream = request.Image.OpenReadStream();

        string imageUrl = await fileService.UploadAsync(
            stream,
            request.Image.FileName,
            request.Image.ContentType,
            folderName,
            cancellationToken);

        // 2. Entity Oluşturma
        var banner = new Banner(
            request.Title,
            request.Description,
            imageUrl,
            request.TargetUrl,
            request.Order,
            companyId
        );

        bannerRepository.Add(banner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Succeed(banner.Id);
    }
}