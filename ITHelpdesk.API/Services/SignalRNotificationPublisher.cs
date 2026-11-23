
using ITHelpdesk.Application.DTOs.Notifications;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ITHelpdesk.API.Services;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(
        Guid userId,
        NotificationResponse notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user:{userId}")
                .SendAsync("NotificationReceived", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish notification {NotificationId} to user {UserId}",
                notification.Id,
                userId);
        }
    }
}
