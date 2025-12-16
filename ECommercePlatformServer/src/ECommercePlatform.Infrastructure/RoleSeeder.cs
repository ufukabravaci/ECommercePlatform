using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Infrastructure;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();

        string[] roles = { RoleConsts.SuperAdmin, RoleConsts.CompanyOwner, RoleConsts.Employee, RoleConsts.Customer };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }
    }
}
