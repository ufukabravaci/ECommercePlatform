namespace ECommercePlatform.Application.DTOs;

public sealed record EmployeeDto(
    Guid UserId,          // <--- Kritik ID bu
    string FirstName,
    string LastName,
    string Email,
    List<string> Roles,        // Kişinin rolleri
    List<string> Permissions   // Kişiye özel ekstra yetkiler
);
