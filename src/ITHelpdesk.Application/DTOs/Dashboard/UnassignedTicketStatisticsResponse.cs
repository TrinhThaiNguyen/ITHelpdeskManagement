
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Dashboard;

public class UnassignedTicketStatisticsResponse
{
    public int TotalUnassignedTickets { get; set; }

    public int LowPriorityCount { get; set; }

    public int MediumPriorityCount { get; set; }

    public int HighPriorityCount { get; set; }

    public int CriticalPriorityCount { get; set; }

    public List<UnassignedTicketByPriorityResponse> ByPriority { get; set; }
        = new();
}

public class UnassignedTicketByPriorityResponse
{
    public TicketPriority Priority { get; set; }

    public int TicketCount { get; set; }
}
