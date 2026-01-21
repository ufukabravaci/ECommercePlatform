namespace ECommercePlatform.MvcAdmin.DTOs.Orders;

public sealed class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}
