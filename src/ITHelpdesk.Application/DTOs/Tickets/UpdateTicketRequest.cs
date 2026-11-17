namespace ITHelpdesk.Application.DTOs.Tickets;

public class UpdateTicketRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; }

    public Guid CategoryId { get; set; }

    public Guid DepartmentId { get; set; }
}