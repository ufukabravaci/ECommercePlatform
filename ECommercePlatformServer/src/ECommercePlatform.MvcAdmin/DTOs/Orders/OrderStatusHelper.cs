namespace ECommercePlatform.MvcAdmin.DTOs.Orders;

public static class OrderStatusHelper
{
    public static string GetStatusBadgeClass(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" or "beklemede" => "pending",
            "confirmed" or "onaylandı" => "info",
            "processing" or "hazırlanıyor" => "warning",
            "shipped" or "kargoda" or "kargoya verildi" => "primary",
            "delivered" or "teslim edildi" => "active",
            "cancelled" or "iptal edildi" => "inactive",
            "refunded" or "iade edildi" => "refunded",
            _ => "secondary"
        };
    }

    public static string GetStatusIcon(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" or "beklemede" => "fa-clock",
            "confirmed" or "onaylandı" => "fa-check-circle",
            "processing" or "hazırlanıyor" => "fa-cog fa-spin",
            "shipped" or "kargoda" or "kargoya verildi" => "fa-truck",
            "delivered" or "teslim edildi" => "fa-check-double",
            "cancelled" or "iptal edildi" => "fa-times-circle",
            "refunded" or "iade edildi" => "fa-undo",
            _ => "fa-question-circle"
        };
    }

    public static string GetStatusDisplayText(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" => "Beklemede",
            "confirmed" => "Onaylandı",
            "processing" => "Hazırlanıyor",
            "shipped" => "Kargoda",
            "delivered" => "Teslim Edildi",
            "cancelled" => "İptal Edildi",
            "refunded" => "İade Edildi",
            _ => status ?? "Bilinmiyor"
        };
    }

    public static int GetStatusValue(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" or "beklemede" => 0,
            "confirmed" or "onaylandı" => 1,
            "processing" or "hazırlanıyor" => 2,
            "shipped" or "kargoda" => 3,
            "delivered" or "teslim edildi" => 4,
            "cancelled" or "iptal edildi" => 5,
            "refunded" or "iade edildi" => 6,
            _ => 0
        };
    }

    public static List<(int Value, string Text)> GetAllStatuses()
    {
        return new List<(int, string)>
        {
            (0, "Beklemede"),
            (1, "Onaylandı"),
            (2, "Hazırlanıyor"),
            (3, "Kargoda"),
            (4, "Teslim Edildi"),
            (5, "İptal Edildi"),
            (6, "İade Edildi")
        };
    }
}
