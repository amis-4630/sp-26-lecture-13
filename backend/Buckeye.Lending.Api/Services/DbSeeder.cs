using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Services;

/// <summary>
/// Seeds default roles and users on startup, and assigns loan ownership.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<LendingContext>();

        // Seed roles
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
            }
        }

        // Seed admin user
        ApplicationUser? admin = await userManager.FindByEmailAsync("admin@buckeye.edu");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@buckeye.edu",
                Email = "admin@buckeye.edu",
                FullName = "Admin User"
            };
            await userManager.CreateAsync(admin, "AdminPass123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed regular user
        ApplicationUser? regularUser = await userManager.FindByEmailAsync("user@buckeye.edu");
        if (regularUser is null)
        {
            regularUser = new ApplicationUser
            {
                UserName = "user@buckeye.edu",
                Email = "user@buckeye.edu",
                FullName = "Regular User"
            };
            await userManager.CreateAsync(regularUser, "UserPass123");
            await userManager.AddToRoleAsync(regularUser, "User");
        }

        // Assign ownership to existing seed loans (admin owns 1-4, user owns 5-8)
        var unownedLoans = await context.LoanApplications
            .Where(l => l.OwnerUserId == null)
            .ToListAsync();

        foreach (var loan in unownedLoans)
        {
            loan.OwnerUserId = loan.Id <= 4 ? admin.Id : regularUser.Id;
        }

        if (unownedLoans.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }
}
