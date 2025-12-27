namespace ECommercePlatform.MvcAdmin.DTOs.Products;

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    bool IsMain
);