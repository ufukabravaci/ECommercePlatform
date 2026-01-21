using ECommercePlatform.Domain.Abstractions;

namespace ECommercePlatform.Domain.Companies;

public sealed class CompanyInvitation : Entity, IMultiTenantEntity
{
    private CompanyInvitation() { }

    public CompanyInvitation(Guid companyId, string email, string role)
    {
        CompanyId = companyId;
        Email = email;
        Role = role;
        Token = Guid.NewGuid().ToString(); // Güvenlik için random token
        InvitationDate = DateTimeOffset.Now;
        ExpirationDate = DateTimeOffset.Now.AddDays(3); // 3 gün geçerli
        Status = InvitationStatus.Pending;
    }

    public Guid CompanyId { get; private set; }
    public string Email { get; private set; } = default!;
    public string Token { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public DateTimeOffset InvitationDate { get; private set; }
    public DateTimeOffset ExpirationDate { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void MarkAsAccepted()
    {
        if (Status != InvitationStatus.Pending) throw new ApplicationException("Davet zaten işlem görmüş.");
        if (DateTimeOffset.Now > ExpirationDate) throw new ApplicationException("Davet süresi dolmuş.");

        Status = InvitationStatus.Accepted;
        CompletedAt = DateTimeOffset.Now;
    }

    public void MarkAsRejected()
    {
        if (Status != InvitationStatus.Pending) return;
        Status = InvitationStatus.Rejected;
        CompletedAt = DateTimeOffset.Now;
    }
}

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}