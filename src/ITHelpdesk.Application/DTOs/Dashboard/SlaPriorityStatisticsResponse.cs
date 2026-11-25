using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Dashboard;

public class SlaPriorityStatisticsResponse
{
    public TicketPriority Priority { get; set; }

    public int TotalTickets { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedCount { get; set; }
}