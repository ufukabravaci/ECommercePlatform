namespace ECommercePlatform.Domain.Baskets;

public sealed class BasketItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal PriceAmount { get; set; } // Money VO yerine primitive tutmak Redis serileştirme için daha kolaydır
    public string PriceCurrency { get; set; } = default!;
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}
