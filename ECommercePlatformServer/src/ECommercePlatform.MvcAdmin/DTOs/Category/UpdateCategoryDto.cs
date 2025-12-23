namespace ECommercePlatform.MvcAdmin.DTOs.Category;

// update
public record UpdateCategoryRequestDto(
    Guid Id,
    string Name,
    Guid? ParentId
);
