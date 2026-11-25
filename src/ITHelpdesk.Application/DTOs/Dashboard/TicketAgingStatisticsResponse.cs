
namespace ITHelpdesk.Application.DTOs.Dashboard;

public class TicketAgingStatisticsResponse
{
    public int TotalOpenTickets { get; set; }

    public int FromZeroToOneDay { get; set; }

    public int FromOneToThreeDays { get; set; }

    public int FromThreeToSevenDays { get; set; }

    public int OverSevenDays { get; set; }
}
