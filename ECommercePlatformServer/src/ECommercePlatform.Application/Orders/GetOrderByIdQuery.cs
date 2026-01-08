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


[Permission(PermissionConsts.ReadOrder)]
public sealed record GetOrderByIdQuery(string OrderNumber) : IRequest<Result<OrderDetailDto>>;

public sealed class GetOrderByIdQueryHandler(
    IRepository<Order> orderRepository
) : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // ProjectToType kullanırsak Include yapmamıza gerek kalmaz, 
        // Mapster ilişkileri configden anlayıp kendi join'lerini kurar.
        var orderDto = await orderRepository.AsQueryable()
            .Where(x => x.OrderNumber == request.OrderNumber)
            .ProjectToType<OrderDetailDto>() // Include(Items) demeye gerek yok, Mapster halleder.
            .FirstOrDefaultAsync(cancellationToken);

        if (orderDto is null)
            return Result<OrderDetailDto>.Failure("Sipariş bulunamadı.");

        return orderDto;
    }
}