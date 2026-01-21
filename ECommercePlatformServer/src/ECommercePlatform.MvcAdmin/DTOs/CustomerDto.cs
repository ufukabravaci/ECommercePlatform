namespace ECommercePlatform.MvcAdmin.DTOs;

public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}
