using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.WebAPI;

public static class ExtensionMethods
{
    public static async Task CreateFirstUser(this WebApplication app)
    {
        // (DI container'dan servis çekmek için)
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var user = await userManager.FindByNameAsync("admin");

        if (user is null)
        {
            user = new User(
                firstName: "Ufuk",
                lastName: "Abravacı",
                email: "ufukabravaci@gmail.com",
                userName: "admin"
            );
            user.EmailConfirmed = true;
            var result = await userManager.CreateAsync(user, "Admin123*");

            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    await roleManager.CreateAsync(new AppRole("SuperAdmin"));
                }
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
            else
            {
                // Loglama yapılabilir
            }
        }
    }
}