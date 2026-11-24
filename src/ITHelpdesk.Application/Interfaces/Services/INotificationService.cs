using ITHelpdesk.Application.DTOs.Notifications;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? referenceId = null);

    Task<List<NotificationResponse>> GetMyNotificationsAsync(
        Guid userId,
        int page,
        int pageSize);

    Task<NotificationUnreadCountResponse> GetUnreadCountAsync(
        Guid userId);

    Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId);

    Task MarkAllAsReadAsync(Guid userId);

    Task NotifyTicketCreatedAsync(Guid ticketId, string ticketTitle);

    Task NotifyTicketAssignedAsync(
        Guid assigneeUserId,
        Guid ticketId,
        string ticketTitle);
}