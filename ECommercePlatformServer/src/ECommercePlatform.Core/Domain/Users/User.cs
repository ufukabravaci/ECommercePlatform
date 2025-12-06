using ECommercePlatform.Core.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Core.Domain.Users;

public sealed class User : IdentityUser<Guid>, IAuditableEntity
{
    public User()
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.Now;
        IsActive = true;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    // Helper Constructor
    public User(string firstName, string lastName, string email, string userName) : this()
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserName = userName;
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = userName.ToUpperInvariant();
    }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public int? CompanyId { get; set; } //boşsa süperadmin veya customer

    // Refresh Token İlişkisi
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

    // --- Audits ---
    #region Audit
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    #endregion

    public void SetStatus(bool isActive) => IsActive = isActive;

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.Now;
        EmailConfirmed = false;
    }
}