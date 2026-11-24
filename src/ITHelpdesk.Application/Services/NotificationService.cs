using ITHelpdesk.Application.DTOs.Notifications;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly INotificationRecipientRepository _notificationRecipientRepository;

    public NotificationService(
        INotificationRepository notificationRepository, INotificationPublisher notificationPublisher, INotificationRecipientRepository notificationRecipientRepository)
    {
        _notificationRepository = notificationRepository;
        _notificationPublisher = notificationPublisher;
        _notificationRecipientRepository = notificationRecipientRepository;
    }


    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? referenceId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type.ToString(),
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // 1. L?u notification vào database
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // 2. Chuy?n entity thành DTO
        var response = new NotificationResponse
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            ReferenceId = notification.ReferenceId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };

        // 3. G?i notification realtime ??n ng??i nh?n
        await _notificationPublisher.PublishAsync(userId, response);
    }


    public async Task<List<NotificationResponse>> GetMyNotificationsAsync(
        Guid userId,
        int page,
        int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var notifications =
            await _notificationRepository.GetByUserIdAsync(
                userId,
                page,
                pageSize);

        return notifications.Select(x => new NotificationResponse
        {
            Id = x.Id,
            Title = x.Title,
            Message = x.Message,
            Type = x.Type,
            ReferenceId = x.ReferenceId,
            IsRead = x.IsRead,
            CreatedAt = x.CreatedAt,
            ReadAt = x.ReadAt
        }).ToList();
    }

    public async Task<NotificationUnreadCountResponse> GetUnreadCountAsync(
        Guid userId)
    {
        var count =
            await _notificationRepository.GetUnreadCountAsync(userId);

        return new NotificationUnreadCountResponse
        {
            UnreadCount = count
        };
    }

    public async Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId)
    {
        var notification =
            await _notificationRepository.GetByIdAndUserIdAsync(
                notificationId,
                userId);

        if (notification == null)
            return false;

        if (notification.IsRead)
            return true;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _notificationRepository.SaveChangesAsync();

        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task NotifyTicketCreatedAsync(
        Guid ticketId,
        string ticketTitle)
    {
        var recipientIds =
            await _notificationRecipientRepository.GetUserIdsByRolesAsync(
                new[] { "Admin", "ITStaff" });

        foreach (var userId in recipientIds)
        {
            await CreateAsync(
                userId,
                "New Ticket Created",
                $"A new ticket has been created: {ticketTitle}",
                NotificationType.TicketCreated,
                ticketId);
        }
    }

    public async Task NotifyTicketAssignedAsync(
        Guid assigneeUserId,
        Guid ticketId,
        string ticketTitle)
    {
        await CreateAsync(
            assigneeUserId,
            "Ticket Assigned",
            $"You have been assigned a new ticket: {ticketTitle}",
            NotificationType.TicketAssigned,
            ticketId);
    }
}