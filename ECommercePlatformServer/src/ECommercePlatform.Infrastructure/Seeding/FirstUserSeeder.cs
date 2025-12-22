using ECommercePlatform.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Infrastructure.Seeding;

public static class FirstUserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();

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
                // Rol zaten RoleSeeder'da oluşturulduğu için burada sadece kontrol ve atama yapıyoruz
                if (await roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(user, "SuperAdmin");
                }
            }
        }
    }
}
