using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Baskets;

public sealed record GetBasketQuery : IRequest<Result<CustomerBasket>>;

public sealed class GetBasketQueryHandler(
    IBasketRepository basketRepository,
    IUserContext userContext
) : IRequestHandler<GetBasketQuery, Result<CustomerBasket>>
{
    public async Task<Result<CustomerBasket>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();
        var basket = await basketRepository.GetBasketAsync(userId, cancellationToken);

        // Eğer sepet yoksa boş bir sepet dönelim (Frontend null check ile uğraşmasın)
        return basket ?? new CustomerBasket(userId);
    }
}
