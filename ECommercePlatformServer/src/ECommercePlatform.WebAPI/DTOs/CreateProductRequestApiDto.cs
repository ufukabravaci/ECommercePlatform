namespace ECommercePlatform.WebAPI.DTOs;

public sealed record CreateProductRequestApiDto
{
    public string Name { get; init; } = default!;
    public string Sku { get; init; } = default!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TRY";
    public int Stock { get; init; }
    public string Description { get; init; } = default!;
    public Guid CategoryId { get; init; }
    public IFormFileCollection? Files { get; init; }
}
