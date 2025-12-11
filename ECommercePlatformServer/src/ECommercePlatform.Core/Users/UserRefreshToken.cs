using ECommercePlatform.Domain.Abstractions;

namespace ECommercePlatform.Domain.Users;

public sealed class UserRefreshToken : Entity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = default!;
    public DateTimeOffset Expiration { get; set; }

    // Token Rotation Güvenliği
    public string? RevokedByIp { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public User User { get; set; } = default!;

    public bool IsExpired => DateTimeOffset.Now >= Expiration;
    public bool IsValid => RevokedAt == null && !IsExpired;
}