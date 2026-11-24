
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketReopenRequestResponse
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public TicketReopenRequestStatus Status { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? ReviewReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
