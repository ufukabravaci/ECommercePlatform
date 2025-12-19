using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ECommercePlatform.Infrastructure;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();
        await CreateRoleAsync(roleManager, RoleConsts.SuperAdmin);
        await CreateRoleAsync(roleManager, RoleConsts.CompanyOwner);
        await CreateRoleAsync(roleManager, RoleConsts.Employee);
        await CreateRoleAsync(roleManager, RoleConsts.Customer);

        // 2. Rollerin Yetkilerini Ata
        var companyOwnerRole = await roleManager.FindByNameAsync(RoleConsts.CompanyOwner);
        if (companyOwnerRole != null)
        {
            await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateCompany);
            await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadCompany);
            await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteCompany);
            //Cache temizleme
            await cache.RemoveAsync($"Role_{RoleConsts.CompanyOwner}_Permissions");
        }

        var employeeRole = await roleManager.FindByNameAsync(RoleConsts.Employee);
        if (employeeRole != null)
        {
            //Cache temizleme
            await cache.RemoveAsync($"Role_{RoleConsts.Employee}_Permissions");
        }
        var customerRole = await roleManager.FindByNameAsync(RoleConsts.Customer);
        if (customerRole != null)
        {
            //Cache temizleme
            await cache.RemoveAsync($"Role_{RoleConsts.Customer}_Permissions");
        }


    }

    private static async Task CreateRoleAsync(RoleManager<AppRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new AppRole(roleName));
    }

    private static async Task AddPermissionToRole(RoleManager<AppRole> roleManager, AppRole role, string permission)
    {
        // Permission tipi "Permission" olan bir Claim ekliyoruz
        //bu roller aspnetroleclaims tablosunda tutuluyor
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(c => c.Type == ClaimTypesConst.Permission && c.Value == permission))
        {
            await roleManager.AddClaimAsync(role, new Claim(ClaimTypesConst.Permission, permission));
        }
    }
}
