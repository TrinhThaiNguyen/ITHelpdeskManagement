using ITHelpdesk.Application.DTOs.Dashboard;
using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/dashboard/export")]
[Authorize(Roles = "Admin,ITStaff")]
public class DashboardExportController : ControllerBase
{
    private readonly IDashboardExportService _exportService;

    public DashboardExportController(
        IDashboardExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> ExportSummary(
        [FromQuery] DashboardExportRequest request,
        CancellationToken cancellationToken)
    {
        var file = await _exportService.ExportDashboardSummaryAsync(
            request, cancellationToken);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"dashboard-summary_{request.FromDate:yyyyMMdd}_{request.ToDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("sla-performance")]
    public async Task<IActionResult> ExportSlaPerformance(
        [FromQuery] DashboardExportRequest request,
        CancellationToken cancellationToken)
    {
        var file = await _exportService.ExportSlaPerformanceAsync(
            request, cancellationToken);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"sla-performance_{request.FromDate:yyyyMMdd}_{request.ToDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("staff-performance")]
    public async Task<IActionResult> ExportStaffPerformance(
        [FromQuery] DashboardExportRequest request,
        CancellationToken cancellationToken)
    {
        var file = await _exportService.ExportStaffPerformanceAsync(
            request, cancellationToken);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"staff-performance_{request.FromDate:yyyyMMdd}_{request.ToDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("department-performance")]
    public async Task<IActionResult> ExportDepartmentPerformance(
        [FromQuery] DashboardExportRequest request,
        CancellationToken cancellationToken)
    {
        var file = await _exportService.ExportDepartmentPerformanceAsync(
            request, cancellationToken);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"department-performance_{request.FromDate:yyyyMMdd}_{request.ToDate:yyyyMMdd}.xlsx");
    }
}