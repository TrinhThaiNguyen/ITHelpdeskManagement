namespace ITHelpdesk.Application.DTOs.Dashboard;

public class TicketDepartmentStatisticsResponse
{
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int TotalTickets { get; set; }

    public double Percentage { get; set; }
}