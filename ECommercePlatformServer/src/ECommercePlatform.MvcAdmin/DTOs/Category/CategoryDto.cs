namespace ECommercePlatform.MvcAdmin.DTOs.Category;
//list
public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? ParentName
);
