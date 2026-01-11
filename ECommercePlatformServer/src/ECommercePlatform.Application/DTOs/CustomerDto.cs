namespace ECommercePlatform.Application.DTOs;

public sealed record CustomerDto(
    Guid Id, // User Id
    string FirstName,
    string LastName,
    string Email,
    DateTimeOffset CreatedAt,
    bool IsActive
);
