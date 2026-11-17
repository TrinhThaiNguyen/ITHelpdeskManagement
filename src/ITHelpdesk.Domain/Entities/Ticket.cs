using ITHelpdesk.Domain.Common;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public Guid CategoryId { get; set; }

    public TicketCategory Category { get; set; } = null!;

    public Guid DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public Guid RequesterId { get; set; }

    public Employee Requester { get; set; } = null!;

    public Guid? AssigneeId { get; set; }

    public Employee? Assignee { get; set; }

    public ICollection<TicketComment> Comments { get; set; }
        = new List<TicketComment>();

    public ICollection<TicketHistory> Histories { get; set; }
        = new List<TicketHistory>();

    public ICollection<TicketAttachment> Attachments { get; set; }
    = new List<TicketAttachment>();

    public bool IsDeleted { get; set; } = false;
}
