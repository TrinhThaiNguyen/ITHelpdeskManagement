
using ITHelpdesk.Application.DTOs.Dashboard;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface IDashboardExportService
{
    Task<byte[]> ExportDashboardSummaryAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportSlaPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportStaffPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportDepartmentPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default);
}
