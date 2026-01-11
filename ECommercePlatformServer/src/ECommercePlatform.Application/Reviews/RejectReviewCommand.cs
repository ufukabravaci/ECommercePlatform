using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

[Permission(PermissionConsts.ManageReview)]
public sealed record RejectReviewCommand(Guid Id) : IRequest<Result<string>>;

public sealed class RejectReviewCommandHandler(
    IRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RejectReviewCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.Id, cancellationToken);

        if (review is null) return Result<string>.Failure("Yorum bulunamadı.");

        review.Reject(); // Domain Behavior: IsApproved = false
                         // İstersen tamamen silmek için: review.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Succeed("Yorum reddedildi (yayından kaldırıldı).");
    }
}

