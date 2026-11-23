
using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class TicketSla : BaseEntity
{
    public Guid TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public Guid SlaPolicyId { get; set; }

    public SlaPolicy SlaPolicy { get; set; } = null!;

    public DateTime ResponseDeadline { get; set; }

    public DateTime ResolutionDeadline { get; set; }

    public DateTime? FirstResponseAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool ResponseBreached { get; set; }

    public bool ResolutionBreached { get; set; }
}
