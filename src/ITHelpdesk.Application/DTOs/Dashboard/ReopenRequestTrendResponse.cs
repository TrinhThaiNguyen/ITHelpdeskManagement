namespace ITHelpdesk.Application.DTOs.Dashboard;

public class ReopenRequestTrendResponse
{
    public DateOnly Date { get; set; }

    public int PendingCount { get; set; }

    public int ApprovedCount { get; set; }

    public int RejectedCount { get; set; }

    public int TotalRequests { get; set; }
}