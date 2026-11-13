using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;

    public Ticket Ticket { get; set; } = null!;
}
