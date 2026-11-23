using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }

    public async Task<List<Notification>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(x => x.UserId == userId && !x.IsRead);
    }

    public async Task<Notification?> GetByIdAndUserIdAsync(
        Guid notificationId,
        Guid userId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.Id == notificationId &&
                x.UserId == userId);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, DateTime.UtcNow));
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}