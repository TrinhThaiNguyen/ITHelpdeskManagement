using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class TicketHistory : BaseEntity
{
    public Guid TicketId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangedBy { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
