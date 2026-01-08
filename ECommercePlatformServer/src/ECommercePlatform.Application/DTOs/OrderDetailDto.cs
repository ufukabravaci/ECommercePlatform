namespace ECommercePlatform.Application.DTOs;


// Detay için (Kapsamlı)
public sealed record OrderDetailDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = default!;
    public decimal TotalAmount { get; init; }

    // Address (Flattened)
    public string ShippingCity { get; init; } = default!;
    public string ShippingDistrict { get; init; } = default!;
    public string ShippingStreet { get; init; } = default!;
    public string ShippingZipCode { get; init; } = default!;
    public string ShippingFullAddress { get; init; } = default!;

    public List<OrderItemDto> Items { get; init; } = new();
}