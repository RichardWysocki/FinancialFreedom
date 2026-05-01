using Microsoft.AspNetCore.Identity;

namespace FinancialFreedom.Server.Data;

/// <summary>Development-only demo account. Do not enable in production.</summary>
public static class IdentityDataSeeder
{
    public const string DemoEmail = "demo@local";
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (await userManager.FindByEmailAsync(DemoEmail) is not null)
            return;

        const string demoRole = "User";
        if (await roleManager.FindByNameAsync(demoRole) is null)
        {
            var roleCreate = await roleManager.CreateAsync(new IdentityRole(demoRole));
            if (!roleCreate.Succeeded)
                return;
        }

        var user = new ApplicationUser
        {
            UserName = DemoEmail,
            Email = DemoEmail,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
            return;

        if (!await userManager.IsInRoleAsync(user, demoRole))
            await userManager.AddToRoleAsync(user, demoRole);
    }
}
