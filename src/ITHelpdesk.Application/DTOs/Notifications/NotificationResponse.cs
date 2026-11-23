namespace ITHelpdesk.Application.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Guid? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }
}