namespace ITHelpdesk.Application.DTOs.Dashboard;

public class ReopenRequestSummaryResponse
{
    public int TotalRequests { get; set; }

    public int PendingCount { get; set; }

    public int ApprovedCount { get; set; }

    public int RejectedCount { get; set; }

    public double ApprovalRate { get; set; }

    public double RejectionRate { get; set; }

    public double AverageReviewTimeHours { get; set; }
}