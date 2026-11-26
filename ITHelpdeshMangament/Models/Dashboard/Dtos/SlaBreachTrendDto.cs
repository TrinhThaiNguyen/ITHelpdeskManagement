
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class SlaBreachTrendDto
{
    public DateOnly Date { get; set; }

    public int ResponseBreachedCount { get; set; }

    public int ResolutionBreachedCount { get; set; }

    public int TotalBreachedCount { get; set; }
}
