namespace ECommercePlatform.Application.DTOs;

public sealed record BannerDto(
    Guid Id,
    string Title,
    string Description,
    string ImageUrl,
    string TargetUrl,
    int Order);
