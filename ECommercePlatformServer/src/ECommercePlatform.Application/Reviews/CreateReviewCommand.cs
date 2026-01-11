using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Reviews;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

[Permission(PermissionConsts.CreateReview)]
public sealed record CreateReviewCommand(
    Guid ProductId,
    int Rating,
    string Comment
) : IRequest<Result<Guid>>;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateReviewCommandHandler(
    IRepository<Review> reviewRepository,
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext
) : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Ürün Var mı?
        var product = await productRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return Result<Guid>.Failure("Ürün bulunamadı.");

        // TODO Opsiyonel: Müşteri bu ürünü satın almış mı kontrolü eklenebilir. (OrderRepository check)
        // Şimdilik herkes yorum yapabilir varsayıyoruz.

        var userId = userContext.GetUserId();

        // 2. Review Oluştur
        var review = new Review(
            request.ProductId,
            userId,
            product.CompanyId,
            request.Rating,
            request.Comment
        );

        reviewRepository.Add(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Succeed(review.Id);
    }
}