using global::ECommercePlatform.MvcAdmin.DTOs;
using global::ECommercePlatform.MvcAdmin.DTOs.Orders;

namespace ECommercePlatform.MvcAdmin.Models.Orders;

public class OrderListViewModel
{
    public PageResult<OrderListDto> Orders { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
}
