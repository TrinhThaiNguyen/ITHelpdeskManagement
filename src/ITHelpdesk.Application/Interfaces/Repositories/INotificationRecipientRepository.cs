
namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface INotificationRecipientRepository
{
    Task<List<Guid>> GetUserIdsByRolesAsync(
        IEnumerable<string> roles);

    Task<Guid?> GetUserIdByEmployeeIdAsync(Guid employeeId);
}
