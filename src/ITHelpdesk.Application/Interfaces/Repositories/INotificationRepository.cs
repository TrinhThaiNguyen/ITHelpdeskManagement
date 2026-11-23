using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task<List<Notification>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task<Notification?> GetByIdAndUserIdAsync(
        Guid notificationId,
        Guid userId);

    Task MarkAllAsReadAsync(Guid userId);

    Task SaveChangesAsync();
}