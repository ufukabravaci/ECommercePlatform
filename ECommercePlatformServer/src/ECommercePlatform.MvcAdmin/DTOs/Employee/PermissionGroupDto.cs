namespace ECommercePlatform.MvcAdmin.DTOs.Employee;

public record PermissionGroupDto(
    string GroupName,
    string GroupLabel,
    string GroupIcon,
    List<PermissionDto> Permissions
);