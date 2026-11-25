namespace ITHelpdesk.Application.DTOs.Dashboard;

public class SlaPerformanceSummaryResponse
{
    public int TotalTicketsWithSla { get; set; }

    public int ResponseMetCount { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResponsePendingCount { get; set; }

    public double ResponseComplianceRate { get; set; }

    public int ResolutionMetCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int ResolutionPendingCount { get; set; }

    public double ResolutionComplianceRate { get; set; }
}