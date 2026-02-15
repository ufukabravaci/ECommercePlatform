using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Users;

namespace ECommercePlatform.Domain.Companies;

public sealed class CompanyUser : Entity, IMultiTenantEntity
{
    private CompanyUser() { }

    public CompanyUser(Guid userId, Guid companyId)
    {
        UserId = userId;
        CompanyId = companyId;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public Guid CompanyId { get; private set; }
    public Company Company { get; private set; } = default!;

    // Kullanıcının BU ŞİRKETTEKİ Rolleri (CompanyOwner, Employees vs) AspNetRoles tablosunda tutuluyor.
    // Seed ile bu rollerin permissionları tanımlandı zaten. AspNetRoleClaims tablosunda tutuluyor.
    // Backing Field kullanıyoruz
    private List<string> _roles = new();
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public void AddRole(string role)
    {
        if (!string.IsNullOrWhiteSpace(role) && !_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(string role)
    {
        if (_roles.Contains(role))
        {
            _roles.Remove(role);
        }
    }

    // Kullanıcının BU ŞİRKETTEKİ Ekstra Permissionları (Rollerden bağımsız)
    private List<string> _permissions = new();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    public void AddPermission(string permission)
    {
        if (!string.IsNullOrWhiteSpace(permission) && !_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }
    }

    public void RemovePermission(string permission)
    {
        if (_permissions.Contains(permission))
        {
            _permissions.Remove(permission);
        }
    }
}
