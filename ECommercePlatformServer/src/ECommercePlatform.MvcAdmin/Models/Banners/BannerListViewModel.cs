using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models.Banners;

public class BannerListViewModel
{
    public List<BannerDto> Banners { get; set; } = new();
}
