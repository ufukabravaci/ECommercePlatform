using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ECommercePlatform.Infrastructure.Seeding;

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
            //Company
            bool isAdded = await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateCompany);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadCompany);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteCompany);
            //Category
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateCategory);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateCategory);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadCategory);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteCategory);

            if (isAdded) // Sadece yeni bir claim eklendiyse cache'i uçur
            {
                await cache.RemoveAsync($"Role_{RoleConsts.CompanyOwner}_Permissions");
            }
        }

        var employeeRole = await roleManager.FindByNameAsync(RoleConsts.Employee);
        if (employeeRole != null)
        {
            //isAdded mantığını ekle
            bool isAdded = await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateCompany);
            //Category
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteCategory);
            //Cache temizleme
            if (isAdded)
            {
                await cache.RemoveAsync($"Role_{RoleConsts.Employee}_Permissions");
            }

        }

    }

    private static async Task CreateRoleAsync(RoleManager<AppRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new AppRole(roleName));
    }

    private static async Task<bool> AddPermissionToRole(RoleManager<AppRole> roleManager, AppRole role, string permission)
    {
        // Permission tipi "Permission" olan bir Claim ekliyoruz
        //bu roller db'de aspnetroleclaims tablosunda tutuluyor
        var allClaims = await roleManager.GetClaimsAsync(role);
        if (!allClaims.Any(c => c.Type == ClaimTypesConst.Permission && c.Value == permission))
        {
            await roleManager.AddClaimAsync(role, new Claim(ClaimTypesConst.Permission, permission));
            return true; // Yeni eklendi
        }
        return false;
    }
}
