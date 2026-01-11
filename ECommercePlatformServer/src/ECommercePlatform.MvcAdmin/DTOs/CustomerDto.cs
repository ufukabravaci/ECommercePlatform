namespace ECommercePlatform.MvcAdmin.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTimeOffset CreatedAt,
    bool IsActive
)
{
    public string FullName => $"{FirstName} {LastName}";
}
