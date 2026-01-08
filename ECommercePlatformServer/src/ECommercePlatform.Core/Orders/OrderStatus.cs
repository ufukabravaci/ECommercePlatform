namespace ECommercePlatform.Domain.Orders;

public enum OrderStatus
{
    Pending = 0,    // Ödeme Bekleniyor
    Approved = 1,   // Onaylandı / Hazırlanıyor
    Shipped = 2,    // Kargoda
    Delivered = 3,  // Teslim Edildi
    Cancelled = 4,  // İptal
    Refunded = 5    // İade
}
