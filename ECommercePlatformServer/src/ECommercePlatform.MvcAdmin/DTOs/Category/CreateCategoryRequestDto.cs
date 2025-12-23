namespace ECommercePlatform.MvcAdmin.DTOs.Category;

// create
public record CreateCategoryRequestDto(
    string Name,
    Guid? ParentId
);
