using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.DTOs;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using GenericRepository;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.ReadAllOrders)]
public sealed record GetAllOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string Search = ""
) : IRequest<Result<PageResult<OrderListDto>>>;

public sealed class GetAllOrdersQueryHandler(
    IRepository<Order> orderRepository
) : IRequestHandler<GetAllOrdersQuery, Result<PageResult<OrderListDto>>>
{
    public async Task<Result<PageResult<OrderListDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        // 1. Query Oluştur
        var query = orderRepository.AsQueryable();

        // 2. Arama Filtresi
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.OrderNumber.Contains(request.Search));
        }

        // 3. Toplam Kayıt Sayısı (Pagination için gerekli)
        int totalCount = await query.CountAsync(cancellationToken);

        // 4. Projection (Mapster) & Pagination
        // ProjectToType<T>, AutoMapper'ın ProjectTo'su gibidir. 
        // Sadece ihtiyaç duyulan kolonları SELECT eder (Performans).
        var items = await query
            .OrderByDescending(x => x.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<OrderListDto>() // Mapster Config devreye girer
            .ToListAsync(cancellationToken);

        // 5. Result Dönüşü
        var result = new PageResult<OrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return result;
    }
}