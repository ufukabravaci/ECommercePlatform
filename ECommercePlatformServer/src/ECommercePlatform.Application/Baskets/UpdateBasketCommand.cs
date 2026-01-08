using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Baskets;

public sealed record UpdateBasketCommand(
    List<BasketItem> Items
) : IRequest<Result<CustomerBasket>>;

public sealed class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketCommandValidator()
    {
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.PriceAmount).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class UpdateBasketCommandHandler(
    IBasketRepository basketRepository,
    IUserContext userContext
) : IRequestHandler<UpdateBasketCommand, Result<CustomerBasket>>
{
    public async Task<Result<CustomerBasket>> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.GetUserId();

        var basket = new CustomerBasket(userId)
        {
            Items = request.Items
        };

        var updatedBasket = await basketRepository.UpdateBasketAsync(basket, cancellationToken);

        return updatedBasket;
    }
}
