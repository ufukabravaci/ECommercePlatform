using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Banners;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Banners;


[Permission(PermissionConsts.UpdateBanner)]
public sealed record UpdateBannerCommand(
    Guid Id,
    string Title,
    string Description,
    IFormFile? Image,
    string TargetUrl,
    int Order
) : IRequest<Result<string>>;

public sealed class UpdateBannerCommandHandler(
    IRepository<Banner> bannerRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService
) : IRequestHandler<UpdateBannerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.Id,
            cancellationToken);

        if (banner is null)
            return Result<string>.Failure("Banner bulunamadı.");

        string? newImageUrl = null;

        // Yeni resim varsa işlemi başlat
        if (request.Image is not null)
        {
            // A. Eski Resmi Sil (Clean Up)
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                // URL'den path'i ayıklamak gerekebilir (http://localhost:5000/uploads/...) 
                // LocalFileService Delete metodu path bekliyor.
                try
                {
                    var uri = new Uri(banner.ImageUrl);
                    fileService.Delete(uri.AbsolutePath); // /uploads/banners/...
                }
                catch
                {
                    // URL formatında değilse direkt gönder
                    fileService.Delete(banner.ImageUrl);
                }
            }

            // B. Yeni Resmi Yükle
            string folderName = $"banners/{banner.CompanyId}";
            using var stream = request.Image.OpenReadStream();

            newImageUrl = await fileService.UploadAsync(
                stream,
                request.Image.FileName,
                request.Image.ContentType,
                folderName,
                cancellationToken);
        }

        // Entity Update
        banner.Update(
            request.Title,
            request.Description,
            newImageUrl, // Null ise domain içindeki logic eski URL'i korur
            request.TargetUrl,
            request.Order
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Banner güncellendi.");
    }
}