using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

//kendi yorumunu silebilmesi için managereview yetkisi vermedim.
//managereview yetkisi olan kişi veya customer olan ama sadece yorum sahibi silebilmeli.
[Permission(PermissionConsts.ReadReview)]
public sealed record DeleteReviewCommand(Guid Id) : IRequest<Result<string>>;

public sealed class DeleteReviewCommandHandler(
    IRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext
) : IRequestHandler<DeleteReviewCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();
        var review = await reviewRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.Id,
            cancellationToken);

        if (review is null)
            return Result<string>.Failure("Yorum bulunamadı.");

        bool isAdminOrModerator = await userContext.HasPermissionAsync(PermissionConsts.ManageReview);

        if (review.CustomerId != userId && !isAdminOrModerator)
        {
            return Result<string>.Failure("Bu yorumu silmeye yetkiniz yok.");
        }

        // Entity base sınıfından gelen Soft Delete metodu
        // IsDeleted = true yapar, CreatedAt vs. korunur.
        review.Delete();

        // GenericRepository update çağrısı (Tracking açık olsa bile convention gereği)
        reviewRepository.Update(review);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Yorum başarıyla silindi.");
    }
}