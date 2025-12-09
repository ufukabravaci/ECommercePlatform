using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Domain.Users;

public sealed class AppRole : IdentityRole<Guid>
{
    public AppRole()
    {
        Id = Guid.CreateVersion7();
    }

    public AppRole(string name) : this()
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }
}