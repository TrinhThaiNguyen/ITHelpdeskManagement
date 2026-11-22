namespace ITHelpdesk.Application.DTOs.Dashboard;

public class TicketTrendResponse
{
    public DateOnly Date { get; set; }

    public int TotalTickets { get; set; }
}