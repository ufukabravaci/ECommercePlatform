using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models.Reviews;

public class ReviewListViewModel
{
    public PageResult<ReviewDto> Reviews { get; set; } = new();
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
}