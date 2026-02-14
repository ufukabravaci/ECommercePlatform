namespace ECommercePlatform.MvcAdmin.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Stock,
    Guid CategoryId,
    Guid BrandId,
    string? BrandName,
    string? CategoryName,
    string? MainImageUrl,
    List<ProductImageDto> Images
);
