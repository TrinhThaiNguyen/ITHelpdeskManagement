namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketSlaResponse
{
    public string SlaPolicyName { get; set; } = string.Empty;

    public DateTime ResponseDeadline { get; set; }

    public DateTime ResolutionDeadline { get; set; }

    public DateTime? FirstResponseAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool ResponseBreached { get; set; }

    public bool ResolutionBreached { get; set; }
}