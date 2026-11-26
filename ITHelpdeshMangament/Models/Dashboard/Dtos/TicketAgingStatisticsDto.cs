
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class TicketAgingStatisticsDto
{
    public int TotalOpenTickets { get; set; }

    public int FromZeroToOneDay { get; set; }

    public int FromOneToThreeDays { get; set; }

    public int FromThreeToSevenDays { get; set; }

    public int OverSevenDays { get; set; }
}
