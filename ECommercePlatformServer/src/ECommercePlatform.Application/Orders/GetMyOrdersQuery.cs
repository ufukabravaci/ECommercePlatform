using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.ReadOrder)] // Müşteri yetkisi yeterli
public sealed record GetMyOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PageResult<OrderListDto>>>;

public sealed class GetMyOrdersQueryHandler(
    IRepository<Order> orderRepository,
    IUserContext userContext // Logged in user ID
) : IRequestHandler<GetMyOrdersQuery, Result<PageResult<OrderListDto>>>
{
    public async Task<Result<PageResult<OrderListDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.GetUserId();

        // Sadece giriş yapan kullanıcıya ait siparişler
        var query = orderRepository.AsQueryable()
            .Where(x => x.CustomerId == currentUserId);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<OrderListDto>()
            .ToListAsync(cancellationToken);

        return new PageResult<OrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}