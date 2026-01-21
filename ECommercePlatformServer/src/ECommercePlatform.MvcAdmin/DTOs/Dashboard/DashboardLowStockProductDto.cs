namespace ECommercePlatform.MvcAdmin.DTOs.Dashboard;

public sealed class DashboardLowStockProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
}