using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Domain.Users;

public sealed class User : IdentityUser<Guid>, IAuditableEntity
{
    private User()
    {
    }

    public User(string firstName, string lastName, string email, string userName) : this()
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.Now;
        IsActive = true;
        SecurityStamp = Guid.NewGuid().ToString();
        FirstName = firstName;
        LastName = lastName;
        IsDeleted = false;
        Email = email;
        UserName = userName;
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = userName.ToUpperInvariant();
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public Address? Address { get; private set; }

    private readonly List<UserRefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<UserRefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public IReadOnlyCollection<CompanyUser> CompanyUsers => _companyUsers.AsReadOnly();
    private readonly List<CompanyUser> _companyUsers = new();

    // Metodlar (Behavior)
    #region Methods

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("İsim boş olamaz");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Soyisim boş olamaz");

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
    }

    public void SetAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
    public void SetStatus(bool isActive) => IsActive = isActive;

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTimeOffset.Now;
        EmailConfirmed = false;
    }

    public void AddRefreshToken(UserRefreshToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        _refreshTokens.Add(token);
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens
            .Where(t => t.RevokedAt == null && !t.IsExpired))
        {
            token.RevokedAt = DateTimeOffset.Now;
        }
    }
    #endregion

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
}