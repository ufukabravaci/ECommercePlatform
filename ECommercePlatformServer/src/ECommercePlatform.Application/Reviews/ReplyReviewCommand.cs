using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

[Permission(PermissionConsts.ManageReview)]
public sealed record ReplyReviewCommand(
    Guid ReviewId,
    string Reply
) : IRequest<Result<string>>;

public sealed class ReplyReviewCommandHandler(
    IRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReplyReviewCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ReplyReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.ReviewId,
            cancellationToken);

        if (review is null)
            return Result<string>.Failure("Yorum bulunamadı.");

        review.Reply(request.Reply); // Domain metodu (Otomatik onaylar)

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Yoruma cevap verildi.");
    }
}