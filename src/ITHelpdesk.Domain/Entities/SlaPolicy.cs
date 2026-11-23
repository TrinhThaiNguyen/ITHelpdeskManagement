
using ITHelpdesk.Domain.Common;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Domain.Entities;

public class SlaPolicy : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public int ResponseTimeMinutes { get; set; }

    public int ResolutionTimeMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TicketSla> TicketSlas { get; set; }
        = new List<TicketSla>();
}
