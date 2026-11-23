
using ITHelpdesk.Application.DTOs.Notifications;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface INotificationPublisher
{
    Task PublishAsync(
        Guid userId,
        NotificationResponse notification);
}
