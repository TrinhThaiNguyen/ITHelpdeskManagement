
namespace ITHelpdeshMangament.Models.Tickets;

public class TicketDetailsViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Status { get; set; }

    public int Priority { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public Guid RequesterId { get; set; }

    public Guid? AssigneeId { get; set; }

    public string? AssigneeName { get; set; }

    public string? AssigneeEmail { get; set; }

    public DateTime? AssignedAt { get; set; }

    public List<TicketAssigneeViewModel> AvailableAssignees { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public TicketSlaDetailsViewModel? Sla { get; set; }

    public List<TicketCommentViewModel> Comments { get; set; } = [];

    public List<TicketHistoryViewModel> Histories { get; set; } = [];
}

public class TicketSlaDetailsViewModel
{
    public string SlaPolicyName { get; set; } = string.Empty;

    public DateTime ResponseDeadline { get; set; }

    public DateTime ResolutionDeadline { get; set; }

    public DateTime? FirstResponseAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool ResponseBreached { get; set; }

    public bool ResolutionBreached { get; set; }
}
