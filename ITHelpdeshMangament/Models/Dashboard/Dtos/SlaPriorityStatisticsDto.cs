
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class SlaPriorityStatisticsDto
{
    public int Priority { get; set; }

    public int TotalTickets { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedCount { get; set; }
}
