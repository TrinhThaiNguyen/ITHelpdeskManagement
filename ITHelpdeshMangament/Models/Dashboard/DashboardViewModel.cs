
using ITHelpdeshMangament.Models.Dashboard.Dtos;

namespace ITHelpdeshMangament.Models.Dashboard;

public class DashboardViewModel
{
    // Ticket summary
    public int TotalTickets { get; set; }
    public int NewTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }

    // SLA summary
    public int TotalTicketsWithSla { get; set; }
    public int ResponseOverdueCount { get; set; }
    public int ResolutionOverdueCount { get; set; }

    // Chart data
    public List<TicketPriorityStatisticsDto> PriorityStatistics { get; set; } = new();

    public List<TicketDepartmentStatisticsDto> DepartmentStatistics { get; set; } = new();

    public List<TicketTrendDto> TicketTrends { get; set; } = new();

    public List<TicketStatusDistributionDto> StatusDistribution { get; set; } = new();
}
