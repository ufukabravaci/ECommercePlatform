namespace ECommercePlatform.MvcAdmin.DTOs.Orders;

public enum OrderStatus
{
    Pending = 0,        // Beklemede
    Confirmed = 1,      // Onaylandı
    Processing = 2,     // Hazırlanıyor
    Shipped = 3,        // Kargoya Verildi
    Delivered = 4,      // Teslim Edildi
    Cancelled = 5,      // İptal Edildi
    Refunded = 6        // İade Edildi
}
