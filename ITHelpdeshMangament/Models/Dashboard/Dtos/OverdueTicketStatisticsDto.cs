
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class OverdueTicketStatisticsDto
{
    public int TotalTicketsWithSla { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedTickets { get; set; }
}
