namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class TicketPriorityStatisticsDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}