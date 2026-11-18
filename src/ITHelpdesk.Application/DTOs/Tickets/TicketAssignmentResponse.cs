namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketAssignmentResponse
{
    public Guid TicketId { get; set; }

    public Guid? AssigneeId { get; set; }

    public string? AssigneeName { get; set; }

    public string? AssigneeEmail { get; set; }

    public DateTime? AssignedAt { get; set; }
}