namespace ECommercePlatform.MvcAdmin.DTOs;

public sealed class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? LogoUrl { get; set; }
}
