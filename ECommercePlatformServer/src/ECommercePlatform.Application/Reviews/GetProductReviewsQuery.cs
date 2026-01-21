using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Reviews;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Reviews;



// Public erişim olabilir veya Read yetkisi
public sealed record GetProductReviewsQuery(
    Guid ProductId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PageResult<ReviewDto>>>;

public sealed class GetProductReviewsQueryHandler(
    IRepository<Review> reviewRepository
) : IRequestHandler<GetProductReviewsQuery, Result<PageResult<ReviewDto>>>
{
    public async Task<Result<PageResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = reviewRepository.AsQueryable()
            .Where(x => x.ProductId == request.ProductId && x.IsApproved); // Sadece onaylılar

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            // Mapster Config'e CustomerName mapping eklenmeli (Customer.FirstName + " " + Customer.LastName)
            .ProjectToType<ReviewDto>()
            .ToListAsync(cancellationToken);

        return new PageResult<ReviewDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
