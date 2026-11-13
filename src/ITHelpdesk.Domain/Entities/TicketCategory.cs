using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class TicketCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}