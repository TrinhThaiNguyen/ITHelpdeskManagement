namespace ITHelpdesk.Application.DTOs.Dashboard;

public class SlaBreachTrendResponse
{
    public DateOnly Date { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedCount { get; set; }
}