
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class NotificationRecipientRepository
    : INotificationRecipientRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationRecipientRepository(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<Guid>> GetUserIdsByRolesAsync(
        IEnumerable<string> roles)
    {
        var userIds = new HashSet<Guid>();

        foreach (var role in roles.Distinct())
        {
            var users = await _userManager.GetUsersInRoleAsync(role);

            foreach (var user in users)
            {
                userIds.Add(user.Id);
            }
        }

        return userIds.ToList();
    }

    public async Task<Guid?> GetUserIdByEmployeeIdAsync(
    Guid employeeId)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

        return user?.Id;
    }
}
