using ECommercePlatform.MvcAdmin.DTOs.Employee;

namespace ECommercePlatform.MvcAdmin.DTOs;

public record PermissionGroupDto(
    string GroupName,
    string GroupLabel,
    string GroupIcon,
    List<PermissionDto> Permissions
);
