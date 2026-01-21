namespace ECommercePlatform.Application.DTOs;

public sealed record DashboardStatsDto
{
    // Sipariş İstatistikleri
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ConfirmedOrders { get; init; }
    public int ProcessingOrders { get; init; }
    public int ShippedOrders { get; init; }
    public int DeliveredOrders { get; init; }
    public int CancelledOrders { get; init; }
    public int RefundedOrders { get; init; }
    public decimal TotalRevenue { get; init; }

    // Ürün İstatistikleri
    public int TotalProducts { get; init; }
    public int InStockProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }

    // Diğer İstatistikler
    public int TotalCustomers { get; init; }
    public int TotalCategories { get; init; }
    public int TotalBrands { get; init; }

    // Yorum İstatistikleri
    public int TotalReviews { get; init; }
    public int ApprovedReviews { get; init; }
    public int PendingReviews { get; init; }
    public double AverageRating { get; init; }

    // Son Siparişler
    public List<DashboardRecentOrderDto> RecentOrders { get; init; } = new();

    // Düşük Stoklu Ürünler
    public List<DashboardLowStockProductDto> LowStockProductsList { get; init; } = new();
}

public sealed record DashboardRecentOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public string CustomerName { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string CurrencyCode { get; init; } = "TRY";
    public string Status { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public int ItemCount { get; init; }
}

public sealed record DashboardLowStockProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Sku { get; init; } = default!;
    public int Stock { get; init; }
    public string? ImageUrl { get; init; }
}