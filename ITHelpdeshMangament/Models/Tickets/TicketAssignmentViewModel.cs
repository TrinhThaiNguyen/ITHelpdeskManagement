namespace ITHelpdeshMangament.Models.Tickets;

public class TicketAssignmentViewModel
{
    public Guid TicketId { get; set; }

    public Guid? AssigneeId { get; set; }

    public string? AssigneeName { get; set; }

    public string? AssigneeEmail { get; set; }

    public DateTime? AssignedAt { get; set; }
}