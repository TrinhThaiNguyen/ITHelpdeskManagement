
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

    private bool IsDashboardAuthorized()
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
            return false;
        }

        var isAuthorized = (roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(role =>
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("ITStaff", StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
        {
            HttpContext.Session.Clear();
            return false;
        }

        return true;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? range,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        if (!IsDashboardAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        range = string.IsNullOrWhiteSpace(range)
            ? "Last30Days"
            : range;

        DateOnly selectedFromDate;
        DateOnly selectedToDate;

        switch (range)
        {
            case "Today":
                selectedFromDate = today;
                selectedToDate = today;
                break;

            case "Last7Days":
                selectedFromDate = today.AddDays(-6);
                selectedToDate = today;
                break;

            case "Last30Days":
                selectedFromDate = today.AddDays(-29);
                selectedToDate = today;
                break;

            case "Custom":
                if (!fromDate.HasValue ||
                    !toDate.HasValue ||
                    fromDate.Value > toDate.Value ||
                    toDate.Value.DayNumber - fromDate.Value.DayNumber > 365)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please select a valid date range within 366 days.");

                    selectedFromDate = today.AddDays(-29);
                    selectedToDate = today;
                    range = "Last30Days";
                }
                else
                {
                    selectedFromDate = fromDate.Value;
                    selectedToDate = toDate.Value;
                }
                break;

            default:
                range = "Last30Days";
                selectedFromDate = today.AddDays(-29);
                selectedToDate = today;
                break;
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

            var trends = await _dashboardApiService.GetTrendsAsync(
                selectedFromDate,
                selectedToDate,
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
                    selectedFromDate,
                    selectedToDate,
                    cancellationToken);

            // SLA analytics
            var slaPriority =
                await _dashboardApiService.GetSlaPriorityStatisticsAsync(
                    cancellationToken);

            var slaTrends =
                await _dashboardApiService.GetSlaBreachTrendsAsync(
                    selectedFromDate,
                    selectedToDate,
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
                DepartmentPerformance = departmentPerformance,

                // Dashboard date filter
                FromDate = selectedFromDate,
                ToDate = selectedToDate,
                Range = range,
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
        catch (Exception)
        {
            ViewBag.ErrorMessage =
                "Something went wrong while loading the dashboard. Please try again later.";

            return View(new DashboardViewModel());
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportSummary(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (!IsDashboardAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (fromDate > toDate ||
            toDate.DayNumber - fromDate.DayNumber > 365)
        {
            return BadRequest("Please select a valid date range within 366 days.");
        }

        try
        {
            var fileBytes = await _dashboardApiService.ExportSummaryAsync(
                fromDate,
                toDate,
                cancellationToken);

            var fileName =
                $"Dashboard-Summary-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Backend API. Please try again later.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TempData["ErrorMessage"] =
                "The export request timed out. Please try again.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportSlaPerformance(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (!IsDashboardAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (fromDate > toDate ||
            toDate.DayNumber - fromDate.DayNumber > 365)
        {
            return BadRequest("Please select a valid date range within 366 days.");
        }

        try
        {
            var fileBytes = await _dashboardApiService.ExportSlaPerformanceAsync(
                fromDate,
                toDate,
                cancellationToken);

            var fileName =
                $"SLA-Performance-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Backend API. Please try again later.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TempData["ErrorMessage"] =
                "The export request timed out. Please try again.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportStaffPerformance(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (!IsDashboardAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (fromDate > toDate ||
            toDate.DayNumber - fromDate.DayNumber > 365)
        {
            return BadRequest("Please select a valid date range within 366 days.");
        }

        try
        {
            var fileBytes = await _dashboardApiService.ExportStaffPerformanceAsync(
                fromDate,
                toDate,
                cancellationToken);

            var fileName =
                $"Staff-Performance-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Backend API. Please try again later.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TempData["ErrorMessage"] =
                "The export request timed out. Please try again.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportDepartmentPerformance(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (!IsDashboardAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (fromDate > toDate ||
            toDate.DayNumber - fromDate.DayNumber > 365)
        {
            return BadRequest("Please select a valid date range within 366 days.");
        }

        try
        {
            var fileBytes = await _dashboardApiService.ExportDepartmentPerformanceAsync(
                fromDate,
                toDate,
                cancellationToken);

            var fileName =
                $"Department-Performance-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Backend API. Please try again later.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TempData["ErrorMessage"] =
                "The export request timed out. Please try again.";

            return RedirectToAction(
                nameof(Index),
                new { range = "Last30Days" });
        }
    }


}
