
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class DepartmentPerformanceDto
{
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int TotalTickets { get; set; }

    public int AssignedTickets { get; set; }

    public int UnassignedTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public double AverageResolutionTimeHours { get; set; }

    public double AverageFirstResponseTimeHours { get; set; }

    public int ResolutionSlaMetCount { get; set; }

    public int ResolutionSlaBreachedCount { get; set; }

    public double ResolutionSlaComplianceRate { get; set; }
}
