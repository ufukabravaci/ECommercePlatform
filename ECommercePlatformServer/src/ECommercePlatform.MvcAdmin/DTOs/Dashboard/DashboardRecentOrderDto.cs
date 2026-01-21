namespace ECommercePlatform.MvcAdmin.DTOs.Dashboard;

public sealed class DashboardRecentOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string Status { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
}
