namespace ECommercePlatform.MvcAdmin.DTOs.Products;

public record UpdateProductRequestDto(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Stock,
    Guid CategoryId
);