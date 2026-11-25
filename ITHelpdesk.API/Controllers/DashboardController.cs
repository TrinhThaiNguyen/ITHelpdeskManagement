using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ITStaff")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("by-priority")]
    public async Task<IActionResult> GetByPriority(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetByPriorityAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("by-department")]
    public async Task<IActionResult> GetByDepartment(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetByDepartmentAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dashboardService.GetTrendsAsync(
                fromDate,
                toDate,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("reopen-requests/summary")]
    public async Task<IActionResult> GetReopenRequestSummary(
    CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetReopenRequestSummaryAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reopen-requests/trends")]
    public async Task<IActionResult> GetReopenRequestTrends(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dashboardService.GetReopenRequestTrendsAsync(
                fromDate,
                toDate,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("sla/summary")]
    public async Task<IActionResult> GetSlaPerformanceSummary(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetSlaPerformanceSummaryAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("sla/by-priority")]
    public async Task<IActionResult> GetSlaStatisticsByPriority(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetSlaStatisticsByPriorityAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("sla/trends")]
    public async Task<IActionResult> GetSlaBreachTrends(
    [FromQuery] DateOnly fromDate,
    [FromQuery] DateOnly toDate,
    CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dashboardService.GetSlaBreachTrendsAsync(
                fromDate,
                toDate,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    [HttpGet("ticket-aging")]
    public async Task<IActionResult> GetTicketAgingStatistics(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetTicketAgingStatisticsAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("unassigned-tickets")]
    public async Task<IActionResult> GetUnassignedTicketStatistics(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetUnassignedTicketStatisticsAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("staff-workload")]
    public async Task<IActionResult> GetStaffWorkload(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetStaffWorkloadAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("overdue-tickets")]
    public async Task<IActionResult> GetOverdueTicketStatistics(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetOverdueTicketStatisticsAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("ticket-status-distribution")]
    public async Task<IActionResult> GetTicketStatusDistribution(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService
            .GetTicketStatusDistributionAsync(cancellationToken);

        return Ok(result);
    }


    [HttpGet("staff-performance")]
    public async Task<IActionResult> GetStaffPerformance(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetStaffPerformanceAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("department-performance")]
    public async Task<IActionResult> GetDepartmentPerformance(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDepartmentPerformanceAsync(
            cancellationToken);

        return Ok(result);
    }


}