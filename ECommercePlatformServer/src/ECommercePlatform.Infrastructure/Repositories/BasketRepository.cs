using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ECommercePlatform.Infrastructure.Repositories;

public sealed class BasketRepository(
    IDistributedCache cache,
    ITenantContext tenantContext
    ) : IBasketRepository
{
    // Redis Key: "basket:{CompanyId}:{UserId}"
    private string GetBasketKey(Guid userId)
    {
        if (tenantContext.CompanyId is null || tenantContext.CompanyId == Guid.Empty)
            throw new InvalidOperationException("Basket işlemi için Tenant (Company) bilgisi gereklidir.");

        return $"basket:{tenantContext.CompanyId}:{userId}";
    }

    public async Task<CustomerBasket?> GetBasketAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var key = GetBasketKey(customerId);
        var data = await cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(data))
            return null;

        return JsonSerializer.Deserialize<CustomerBasket>(data);
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken cancellationToken = default)
    {
        var key = GetBasketKey(basket.CustomerId);

        var options = new DistributedCacheEntryOptions
        {
            // Sepet 7 gün boyunca işlem görmezse silinsin
            SlidingExpiration = TimeSpan.FromDays(7)
        };

        var json = JsonSerializer.Serialize(basket);
        await cache.SetStringAsync(key, json, options, cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteBasketAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var key = GetBasketKey(customerId);
        await cache.RemoveAsync(key, cancellationToken);
        return true;
    }
}
