
using Microsoft.AspNetCore.Identity;

namespace ITHelpdesk.Infrastructure.Identity;

public static class RoleSeeder
{
    private static readonly string[] Roles =
    [
        "Admin",
        "ITStaff",
        "Requester"
    ];

    public static async Task SeedAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in Roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var role = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };

                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': {errors}");
                }
            }
        }
    }
}
