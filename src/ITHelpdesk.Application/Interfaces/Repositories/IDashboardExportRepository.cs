
using ITHelpdesk.Application.DTOs.Dashboard;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface IDashboardExportRepository
{
    Task<DashboardSummaryResponse> GetSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<SlaPerformanceSummaryResponse> GetSlaPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffPerformanceResponse>> GetStaffPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentPerformanceResponse>> GetDepartmentPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
