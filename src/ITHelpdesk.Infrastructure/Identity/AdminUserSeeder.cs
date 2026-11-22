
using ITHelpdesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITHelpdesk.Infrastructure.Identity;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await CreateUserAsync(
            userManager,
            configuration["SeedUsers:Admin:Email"],
            configuration["SeedUsers:Admin:Password"],
            "Admin");

        await CreateUserAsync(
            userManager,
            configuration["SeedUsers:ITStaff:Email"],
            configuration["SeedUsers:ITStaff:Password"],
            "ITStaff");
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string role)
    {
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Missing configuration for {role} account.");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Cannot create {role}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Cannot assign {role}: {errors}");
            }
        }
    }
}
