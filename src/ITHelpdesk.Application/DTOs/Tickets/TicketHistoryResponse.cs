
namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketHistoryResponse
{
    public Guid Id { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
