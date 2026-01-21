using ECommercePlatform.MvcAdmin.DTOs.Orders;

namespace ECommercePlatform.MvcAdmin.Models.Orders;

public class OrderDetailViewModel
{
    public OrderDetailDto Order { get; set; } = new();
    public List<OrderStatusOption> AvailableStatuses { get; set; } = new();
}

public class OrderStatusOption
{
    public int Value { get; set; }
    public string Text { get; set; } = default!;
    public bool IsSelected { get; set; }
    public bool IsDisabled { get; set; }
}