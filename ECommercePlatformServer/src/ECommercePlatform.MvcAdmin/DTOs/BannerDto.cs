namespace ECommercePlatform.MvcAdmin.DTOs;


public sealed class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public string TargetUrl { get; set; } = default!;
    public int Order { get; set; }
}
