
using ITHelpdeshMangament.Models.Dashboard.Dtos;

namespace ITHelpdeshMangament.Models.Dashboard;

public class DashboardViewModel
{
    // Dashboard date filter
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }

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

    public string Range { get; set; } = "Last30Days";

    // Existing chart data
    public List<TicketPriorityStatisticsDto> PriorityStatistics { get; set; } = new();
    public List<TicketDepartmentStatisticsDto> DepartmentStatistics { get; set; } = new();
    public List<TicketTrendDto> TicketTrends { get; set; } = new();
    public List<TicketStatusDistributionDto> StatusDistribution { get; set; } = new();

    // Reopen Request Analytics
    public ReopenRequestSummaryDto ReopenRequestSummary { get; set; } = new();
    public List<ReopenRequestTrendDto> ReopenRequestTrends { get; set; } = new();

    // SLA Analytics
    public SlaPerformanceSummaryDto SlaPerformance { get; set; } = new();
    public List<SlaPriorityStatisticsDto> SlaPriorityStatistics { get; set; } = new();
    public List<SlaBreachTrendDto> SlaBreachTrends { get; set; } = new();

    // Staff Performance
    public List<StaffPerformanceDto> StaffPerformance { get; set; } = new();

    // Department Performance
    public List<DepartmentPerformanceDto> DepartmentPerformance { get; set; } = new();

    // Ticket Aging
    public TicketAgingStatisticsDto TicketAging { get; set; } = new();

    // Staff Workload
    public List<StaffWorkloadDto> StaffWorkload { get; set; } = new();

    // Overdue Tickets
    public OverdueTicketStatisticsDto OverdueTickets { get; set; } = new();

    // Unassigned Tickets
    public UnassignedTicketStatisticsDto UnassignedTickets { get; set; } = new();


}
