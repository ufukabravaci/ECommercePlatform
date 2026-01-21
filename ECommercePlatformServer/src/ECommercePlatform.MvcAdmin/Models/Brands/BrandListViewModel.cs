using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models.Brands;

public class BrandListViewModel
{
    public PageResult<BrandDto> Brands { get; set; } = new();
    public string? SearchTerm { get; set; }
}