using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;

[Permission(PermissionConsts.ManageReview)]
public sealed record GetAllReviewsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsApproved = null,
    int? MinRating = null,
    int? MaxRating = null,
    string? SearchTerm = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<Result<PageResult<ReviewDetailDto>>>;

public sealed class GetAllReviewsQueryValidator : AbstractValidator<GetAllReviewsQuery>
{
    public GetAllReviewsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MinRating).InclusiveBetween(1, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxRating).InclusiveBetween(1, 5).When(x => x.MaxRating.HasValue);
    }
}

public sealed class GetAllReviewsQueryHandler(
    IRepository<Review> reviewRepository,
    ITenantContext tenantContext
) : IRequestHandler<GetAllReviewsQuery, Result<PageResult<ReviewDetailDto>>>
{
    public async Task<Result<PageResult<ReviewDetailDto>>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var companyId = tenantContext.CompanyId;
        if (companyId is null)
            return Result<PageResult<ReviewDetailDto>>.Failure("Şirket bilgisi bulunamadı.");

        var query = reviewRepository.AsQueryable()
            .Include(x => x.Product)
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == companyId && !x.IsDeleted);

        // Onay durumu filtresi
        if (request.IsApproved.HasValue)
            query = query.Where(x => x.IsApproved == request.IsApproved.Value);

        // Puan filtresi
        if (request.MinRating.HasValue)
            query = query.Where(x => x.Rating >= request.MinRating.Value);

        if (request.MaxRating.HasValue)
            query = query.Where(x => x.Rating <= request.MaxRating.Value);

        // Arama
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(x =>
                x.Comment.ToLower().Contains(term) ||
                (x.Customer.FirstName + " " + x.Customer.LastName).ToLower().Contains(term) ||
                x.Product.Name.ToLower().Contains(term));
        }

        // Sıralama
        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.SortDescending
                ? query.OrderByDescending(x => x.Rating)
                : query.OrderBy(x => x.Rating),
            "customername" => request.SortDescending
                ? query.OrderByDescending(x => x.Customer.FirstName)
                : query.OrderBy(x => x.Customer.FirstName),
            "productname" => request.SortDescending
                ? query.OrderByDescending(x => x.Product.Name)
                : query.OrderBy(x => x.Product.Name),
            _ => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ReviewDetailDto(
                x.Id,
                x.CustomerId,
                x.Customer.FirstName + " " + x.Customer.LastName,
                x.Rating,
                x.Comment,
                x.CreatedAt,
                x.SellerReply,
                x.SellerRepliedAt,
                x.ProductId,
                x.Product.Name,
                x.IsApproved
            ))
            .ToListAsync(cancellationToken);

        return new PageResult<ReviewDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
