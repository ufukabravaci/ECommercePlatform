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
            //Product
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateProduct);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateProduct);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteProduct);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadProduct);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ManageProductImages);
            //Order
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateOrder);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadOrder);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadAllOrders);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateOrderStatus);
            //Brand
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateBrand);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteBrand);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadBrand);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateBrand);
            //Banner
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateBanner);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteBanner);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadBanner);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateBanner);
            //Review
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.CreateReview);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ManageReview);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadReview);
            //Customer
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadCustomer);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteCustomer);
            //Employee
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadEmployee);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.InviteEmployee);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ManagePermissions);
            //Company
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateShippingSettings);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.UpdateCompany);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.ReadCompany);
            isAdded |= await AddPermissionToRole(roleManager, companyOwnerRole, PermissionConsts.DeleteCompany);

            if (isAdded) // Sadece yeni bir claim eklendiyse cache'i uçur
            {
                await cache.RemoveAsync($"Role_{RoleConsts.CompanyOwner}_Permissions");
            }
        }

        var employeeRole = await roleManager.FindByNameAsync(RoleConsts.Employee);
        if (employeeRole != null)
        {
            bool isAdded = await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateCompany);
            //Category
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCategory);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteCategory);
            //Product
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateProduct);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateProduct);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteProduct);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadProduct);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ManageProductImages);
            //Order
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateOrder);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadOrder);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadAllOrders);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateOrderStatus);
            //Banner
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateBanner);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteBanner);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadBanner);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateBanner);
            //Review
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.CreateReview);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ManageReview);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadReview);
            //Customer
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCustomer);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteCustomer);
            //Company
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateShippingSettings);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.UpdateCompany);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCompany);

            //Cache temizleme
            if (isAdded)
            {
                await cache.RemoveAsync($"Role_{RoleConsts.Employee}_Permissions");
            }

        }
        var customerRole = await roleManager.FindByNameAsync(RoleConsts.Customer);
        if (customerRole != null)
        {
            // Müşterinin yapabilecekleri:
            bool isAdded = await AddPermissionToRole(roleManager, customerRole, PermissionConsts.CreateOrder);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadAllOrders);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadOrder);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadBanner);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadReview);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadCategory);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadProduct);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadBanner);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.UpdateBanner);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.CreateReview);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ManageReview);
            isAdded |= await AddPermissionToRole(roleManager, customerRole, PermissionConsts.ReadReview);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCustomer);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.DeleteCustomer);
            isAdded |= await AddPermissionToRole(roleManager, employeeRole, PermissionConsts.ReadCompany);
            if (isAdded)
            {
                await cache.RemoveAsync($"Role_{RoleConsts.Customer}_Permissions");
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
