namespace ECommercePlatform.Domain.Baskets;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasketAsync(Guid customerId, CancellationToken cancellationToken = default);
}
