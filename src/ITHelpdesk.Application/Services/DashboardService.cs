
using ITHelpdesk.Application.DTOs.Dashboard;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;

namespace ITHelpdesk.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetSummaryAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TicketPriorityStatisticsResponse>>
        GetByPriorityAsync(
            CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetByPriorityAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TicketDepartmentStatisticsResponse>>
        GetByDepartmentAsync(
            CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetByDepartmentAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TicketTrendResponse>> GetTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException(
                "From date cannot be later than To date.");
        }

        var days = toDate.DayNumber - fromDate.DayNumber + 1;

        if (days > 366)
        {
            throw new ArgumentException(
                "Date range cannot exceed 366 days.");
        }

        return _dashboardRepository.GetTrendsAsync(
            fromDate,
            toDate,
            cancellationToken);
    }

    public Task<ReopenRequestSummaryResponse> GetReopenRequestSummaryAsync(
    CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetReopenRequestSummaryAsync(
            cancellationToken);
    }

    public Task<IReadOnlyList<ReopenRequestTrendResponse>>
        GetReopenRequestTrendsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException(
                "From date cannot be later than To date.");
        }

        var days = toDate.DayNumber - fromDate.DayNumber + 1;

        if (days > 366)
        {
            throw new ArgumentException(
                "Date range cannot exceed 366 days.");
        }

        return _dashboardRepository.GetReopenRequestTrendsAsync(
            fromDate,
            toDate,
            cancellationToken);
    }
}
