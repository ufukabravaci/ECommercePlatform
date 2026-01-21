namespace ECommercePlatform.MvcAdmin.DTOs.Employee;

public sealed record EmployeeDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    List<string> Roles,
    List<string> Permissions
);
