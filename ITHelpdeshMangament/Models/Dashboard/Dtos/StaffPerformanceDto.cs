
namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class StaffPerformanceDto
{
    public Guid EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public int TotalAssignedTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public double AverageResolutionTimeHours { get; set; }

    public double AverageFirstResponseTimeHours { get; set; }

    public int ResolutionSlaMetCount { get; set; }

    public int ResolutionSlaBreachedCount { get; set; }

    public double ResolutionSlaComplianceRate { get; set; }
}
