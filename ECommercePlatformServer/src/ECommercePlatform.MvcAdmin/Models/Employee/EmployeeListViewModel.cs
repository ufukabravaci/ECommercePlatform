using ECommercePlatform.MvcAdmin.DTOs.Employee;

namespace ECommercePlatform.MvcAdmin.Models.Employee;

public class EmployeeListViewModel
{
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<PermissionGroupDto> PermissionGroups { get; set; } = new();
    public List<RoleDto> AvailableRoles { get; set; } = new();
}
