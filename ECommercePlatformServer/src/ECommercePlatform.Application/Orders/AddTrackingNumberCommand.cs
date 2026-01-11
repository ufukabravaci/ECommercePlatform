using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.UpdateOrderStatus)]
public sealed record AddTrackingNumberCommand(
    string OrderNumber,
    string TrackingNumber
) : IRequest<Result<string>>;

public sealed class AddTrackingNumberCommandHandler(
    IRepository<Order> orderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddTrackingNumberCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AddTrackingNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByExpressionWithTrackingAsync(
            x => x.OrderNumber == request.OrderNumber,
            cancellationToken);

        if (order is null)
            return Result<string>.Failure("Sipariş bulunamadı.");

        order.SetTrackingNumber(request.TrackingNumber);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Kargo takip numarası eklendi.");
    }
}