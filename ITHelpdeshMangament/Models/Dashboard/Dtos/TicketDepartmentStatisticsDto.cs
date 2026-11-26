namespace ITHelpdeshMangament.Models.Dashboard.Dtos;

public class TicketDepartmentStatisticsDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public double Percentage { get; set; }
}