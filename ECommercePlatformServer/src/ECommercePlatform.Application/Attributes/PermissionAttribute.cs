namespace ECommercePlatform.Application.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PermissionAttribute : Attribute
{
    public string Permission { get; }

    public PermissionAttribute(string permission)
    {
        Permission = permission;
    }
}
