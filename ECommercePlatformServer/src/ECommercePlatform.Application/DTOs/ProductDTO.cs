namespace ECommercePlatform.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Stock,
    Guid CategoryId,
    string? CategoryName,
    string? MainImageUrl,
    List<ProductImageDto> Images
);

public sealed record ProductImageDto(
    Guid Id,
    string ImageUrl,
    bool IsMain
);