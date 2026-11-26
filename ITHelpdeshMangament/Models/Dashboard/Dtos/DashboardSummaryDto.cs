namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class DashboardSummaryDto
{
    public int TotalTickets { get; set; }
    public int NewTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int PendingTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
}