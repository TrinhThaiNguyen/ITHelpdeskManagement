namespace ITHelpdesk.Application.DTOs.Dashboard;

public class TicketPriorityStatisticsResponse
{
    public string Priority { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}