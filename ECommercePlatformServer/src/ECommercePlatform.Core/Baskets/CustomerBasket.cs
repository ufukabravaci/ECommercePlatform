namespace ECommercePlatform.Domain.Baskets;

public sealed class CustomerBasket
{
    public CustomerBasket()
    {
        Items = new List<BasketItem>();
    }

    public CustomerBasket(Guid customerId)
    {
        CustomerId = customerId;
        Items = new List<BasketItem>();
    }

    public Guid CustomerId { get; set; }
    public List<BasketItem> Items { get; set; }

    // Computed property (Redis'e kaydedilmez ama kodda kullanılır)
    public decimal TotalAmount => Items.Sum(x => x.PriceAmount * x.Quantity);
}
