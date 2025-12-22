namespace ECommercePlatform.MvcAdmin.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string ParentName
);
