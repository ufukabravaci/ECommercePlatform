namespace ECommercePlatform.MvcAdmin.DTOs.Dashboard;

public sealed class DashboardStatsDto
{
    // Sipariş İstatistikleri
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int RefundedOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    // Ürün İstatistikleri
    public int TotalProducts { get; set; }
    public int InStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }

    // Diğer İstatistikler
    public int TotalCustomers { get; set; }
    public int TotalCategories { get; set; }
    public int TotalBrands { get; set; }

    // Yorum İstatistikleri
    public int TotalReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int PendingReviews { get; set; }
    public double AverageRating { get; set; }

    // Son Siparişler
    public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();

    // Düşük Stoklu Ürünler
    public List<DashboardLowStockProductDto> LowStockProductsList { get; set; } = new();
}