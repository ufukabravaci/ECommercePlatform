namespace ECommercePlatform.MvcAdmin.DTOs.Orders;

public sealed class OrderDetailDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;  // API string döndürüyor!
    public decimal TotalAmount { get; set; }

    // Address (Flattened) - API'deki isimlerle aynı
    public string ShippingCity { get; set; } = default!;
    public string ShippingDistrict { get; set; } = default!;
    public string ShippingStreet { get; set; } = default!;
    public string ShippingZipCode { get; set; } = default!;
    public string ShippingFullAddress { get; set; } = default!;

    public List<OrderItemDto> Items { get; set; } = new();
}
