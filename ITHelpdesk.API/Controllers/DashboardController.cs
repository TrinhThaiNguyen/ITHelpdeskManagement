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
}