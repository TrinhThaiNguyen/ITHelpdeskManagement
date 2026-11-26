
using ITHelpdeshMangament.Models.Dashboard;
using ITHelpdeshMangament.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdeshMangament.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardApiService _dashboardApiService;

    public DashboardController(
        DashboardApiService dashboardApiService)
    {
        _dashboardApiService = dashboardApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        var expiresAt = HttpContext.Session.GetString("ExpiresAtUtc");
        var roles = HttpContext.Session.GetString("Roles");

        if (string.IsNullOrWhiteSpace(token) ||
            !DateTime.TryParse(
                expiresAt,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var expiration) ||
            expiration <= DateTime.UtcNow)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        var isAuthorized = (roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(role =>
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("ITStaff", StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Existing dashboard data
            var summary = await _dashboardApiService.GetSummaryAsync(
                cancellationToken);

            var slaSummary = await _dashboardApiService.GetSlaSummaryAsync(
                cancellationToken);

            var priority = await _dashboardApiService.GetByPriorityAsync(
                cancellationToken);

            var departments = await _dashboardApiService.GetByDepartmentAsync(
                cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var trends = await _dashboardApiService.GetTrendsAsync(
                today.AddDays(-29),
                today,
                cancellationToken);

            var statusDistribution =
                await _dashboardApiService.GetTicketStatusDistributionAsync(
                    cancellationToken);

            // Reopen request analytics
            var reopenSummary =
                await _dashboardApiService.GetReopenRequestSummaryAsync(
                    cancellationToken);

            var reopenTrends =
                await _dashboardApiService.GetReopenRequestTrendsAsync(
                    cancellationToken);

            // SLA analytics
            var slaPriority =
                await _dashboardApiService.GetSlaPriorityStatisticsAsync(
                    cancellationToken);

            var slaTrends =
                await _dashboardApiService.GetSlaBreachTrendsAsync(
                    cancellationToken);

            // Ticket analytics
            var ticketAging =
                await _dashboardApiService.GetTicketAgingAsync(
                    cancellationToken);

            var unassignedTickets =
                await _dashboardApiService.GetUnassignedTicketsAsync(
                    cancellationToken);

            var overdueTickets =
                await _dashboardApiService.GetOverdueTicketsAsync(
                    cancellationToken);

            // Staff and department analytics
            var staffWorkload =
                await _dashboardApiService.GetStaffWorkloadAsync(
                    cancellationToken);

            var staffPerformance =
                await _dashboardApiService.GetStaffPerformanceAsync(
                    cancellationToken);

            var departmentPerformance =
                await _dashboardApiService.GetDepartmentPerformanceAsync(
                    cancellationToken);

            var model = new DashboardViewModel
            {
                // Ticket summary
                TotalTickets = summary?.TotalTickets ?? 0,
                NewTickets = summary?.NewTickets ?? 0,
                InProgressTickets = summary?.InProgressTickets ?? 0,
                ResolvedTickets = summary?.ResolvedTickets ?? 0,
                ClosedTickets = summary?.ClosedTickets ?? 0,

                // SLA summary
                TotalTicketsWithSla = slaSummary?.TotalTicketsWithSla ?? 0,
                ResponseOverdueCount = slaSummary?.ResponseBreachedCount ?? 0,
                ResolutionOverdueCount = slaSummary?.ResolutionBreachedCount ?? 0,

                // Existing chart data
                PriorityStatistics = priority,
                DepartmentStatistics = departments,
                TicketTrends = trends,
                StatusDistribution = statusDistribution,

                // Reopen request analytics
                ReopenRequestSummary = reopenSummary,
                ReopenRequestTrends = reopenTrends,

                // SLA analytics
                SlaPerformance = slaSummary ?? new(),
                SlaPriorityStatistics = slaPriority,
                SlaBreachTrends = slaTrends,

                // Ticket analytics
                TicketAging = ticketAging,
                UnassignedTickets = unassignedTickets,
                OverdueTickets = overdueTickets,

                // Staff and department analytics
                StaffWorkload = staffWorkload,
                StaffPerformance = staffPerformance,
                DepartmentPerformance = departmentPerformance
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage =
                "Unable to connect to the Backend API. Please check whether the API is running.";

            return View(new DashboardViewModel());
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ErrorMessage =
                "The Backend API request timed out. Please try again.";

            return View(new DashboardViewModel());
        }
    }
}
