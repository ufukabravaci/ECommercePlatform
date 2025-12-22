using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Companies;

namespace ECommercePlatform.Domain.Users;

public sealed class UserRefreshToken : Entity
{
    // Hangi gerçek kullanıcıya ait (Hızlı sorgulama ve RevokeAll işlemleri için gerekli)
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Code { get; set; } = default!;
    public DateTimeOffset Expiration { get; set; }

    // --- DEĞİŞİKLİK BURADA ---
    // Artık her refresh token MUTLAKA bir şirket oturumuna (CompanyUser) bağlıdır.
    // Nullable (?) işaretleri kaldırıldı.
    public Guid CompanyUserId { get; set; }
    public CompanyUser CompanyUser { get; set; } = default!;

    // Token Rotation Güvenliği
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    // Helper Property
    public bool IsExpired => DateTimeOffset.Now >= Expiration;
    public bool IsTokenValid => RevokedAt is null && !IsExpired;
}