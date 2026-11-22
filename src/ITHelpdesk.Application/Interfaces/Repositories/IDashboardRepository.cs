using ITHelpdesk.Application.DTOs.Dashboard;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketPriorityStatisticsResponse>> GetByPriorityAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketDepartmentStatisticsResponse>> GetByDepartmentAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketTrendResponse>> GetTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}