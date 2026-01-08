using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Orders;

[Permission(PermissionConsts.UpdateOrderStatus)]
public sealed record UpdateOrderStatusCommand(
    string OrderNumber,
    OrderStatus NewStatus
) : IRequest<Result<string>>;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Geçersiz sipariş durumu.");
    }
}

public sealed class UpdateOrderStatusCommandHandler(
    IRepository<Order> orderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateOrderStatusCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByExpressionWithTrackingAsync(
            x => x.OrderNumber == request.OrderNumber,
            cancellationToken);

        if (order is null)
            return Result<string>.Failure("Sipariş bulunamadı.");

        order.UpdateStatus(request.NewStatus);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Sipariş durumu güncellendi.");
    }
}
