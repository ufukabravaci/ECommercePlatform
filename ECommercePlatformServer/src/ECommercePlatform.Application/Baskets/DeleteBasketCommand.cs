using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Baskets;

public sealed record DeleteBasketCommand : IRequest<Result<string>>;

public sealed class DeleteBasketCommandHandler(
    IBasketRepository basketRepository,
    IUserContext userContext
) : IRequestHandler<DeleteBasketCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();
        await basketRepository.DeleteBasketAsync(userId, cancellationToken);

        return Result<string>.Succeed("Sepet temizlendi.");
    }
}