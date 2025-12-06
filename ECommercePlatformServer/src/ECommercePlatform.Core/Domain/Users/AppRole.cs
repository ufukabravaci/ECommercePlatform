using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;

namespace ECommercePlatform.Core.Domain.Users;

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