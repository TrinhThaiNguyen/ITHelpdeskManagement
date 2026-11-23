namespace ITHelpdesk.Application.DTOs.Tickets;

public class SlaMonitoringResponse
{
    public int TotalTicketsWithSla { get; set; }

    public int ResponseOverdueCount { get; set; }

    public int ResolutionOverdueCount { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int UpcomingResponseDeadlineCount { get; set; }

    public int UpcomingResolutionDeadlineCount { get; set; }
}