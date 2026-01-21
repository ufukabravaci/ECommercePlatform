namespace ECommercePlatform.MvcAdmin.DTOs.Orders;

public sealed class OrderListDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string CustomerName { get; set; } = default!;
}
