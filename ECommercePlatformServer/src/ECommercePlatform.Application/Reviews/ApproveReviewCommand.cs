using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

[Permission(PermissionConsts.ManageReview)]
public sealed record ApproveReviewCommand(Guid Id) : IRequest<Result<string>>;

public sealed class ApproveReviewCommandHandler(
    IRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ApproveReviewCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == request.Id, cancellationToken);

        if (review is null) return Result<string>.Failure("Yorum bulunamadı.");

        review.Approve(); // Domain Behavior: IsApproved = true

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Succeed("Yorum onaylandı ve yayına alındı.");
    }
}