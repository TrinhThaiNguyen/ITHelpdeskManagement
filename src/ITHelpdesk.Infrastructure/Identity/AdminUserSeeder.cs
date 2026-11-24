using ITHelpdesk.Infrastructure.Identity;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // Create Admin account
        await CreateUserAsync(
            userManager,
            configuration["SeedUsers:Admin:Email"],
            configuration["SeedUsers:Admin:Password"],
            "Admin");

        // Read ITStaff EmployeeId from configuration
        var employeeIdValue =
            configuration["SeedUsers:ITStaff:EmployeeId"];

        if (!Guid.TryParse(employeeIdValue, out var employeeId))
        {
            throw new InvalidOperationException(
                "Invalid or missing SeedUsers:ITStaff:EmployeeId.");
        }

        // Ensure Employee exists and is active
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == employeeId && x.IsActive);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "ITStaff Employee does not exist or is inactive.");
        }

        // Create ITStaff account and link it to Employee
        await CreateUserAsync(
            userManager,
            configuration["SeedUsers:ITStaff:Email"],
            configuration["SeedUsers:ITStaff:Password"],
            "ITStaff",
            employeeId);
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string role,
        Guid? employeeId = null)
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
                EmailConfirmed = true,
                EmployeeId = employeeId
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
        else if (employeeId.HasValue &&
                 user.EmployeeId != employeeId)
        {
            var isAlreadyLinked = await userManager.Users
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.Id != user.Id);

            if (isAlreadyLinked)
            {
                throw new InvalidOperationException(
                    "This Employee is already linked to another account.");
            }

            user.EmployeeId = employeeId;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Cannot update {role} account: {errors}");
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