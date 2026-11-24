
using ITHelpdesk.Domain.Common;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Domain.Entities;

public class TicketReopenRequest : BaseEntity
{
    public Guid TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public TicketReopenRequestStatus Status { get; set; }
        = TicketReopenRequestStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }

    public string? ReviewReason { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
