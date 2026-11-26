
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class UnassignedTicketStatisticsDto
{
    public int TotalUnassignedTickets { get; set; }

    public int LowPriorityCount { get; set; }

    public int MediumPriorityCount { get; set; }

    public int HighPriorityCount { get; set; }

    public int CriticalPriorityCount { get; set; }

    public List<UnassignedTicketByPriorityDto> ByPriority { get; set; } = new();
}
