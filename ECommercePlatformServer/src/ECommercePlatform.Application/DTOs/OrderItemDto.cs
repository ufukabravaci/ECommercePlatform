namespace ECommercePlatform.Application.DTOs;

public sealed record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = default!;
    public int Quantity { get; init; }
    public decimal Total { get; init; }
}
