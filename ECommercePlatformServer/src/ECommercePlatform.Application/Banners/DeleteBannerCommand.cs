using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Banners;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Banners;

[Permission(PermissionConsts.DeleteBanner)]
public sealed record DeleteBannerCommand(Guid Id) : IRequest<Result<string>>;

public sealed class DeleteBannerCommandHandler(
    IRepository<Banner> bannerRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteBannerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await bannerRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.Id,
            cancellationToken);

        if (banner is null)
            return Result<string>.Failure("Banner bulunamadı.");

        banner.Delete(); // Soft Delete

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Banner silindi.");
    }
}