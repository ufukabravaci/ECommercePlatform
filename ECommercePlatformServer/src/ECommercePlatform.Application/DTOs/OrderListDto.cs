namespace ECommercePlatform.Application.DTOs;

// Listeleme için (Hafif)
public sealed record OrderListDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public string CustomerName { get; init; } = default!;
}
