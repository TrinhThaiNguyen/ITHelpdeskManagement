
namespace ITHelpdesk.Application.DTOs.Dashboard;

public class OverdueTicketStatisticsResponse
{
    public int TotalTicketsWithSla { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedTickets { get; set; }
}
