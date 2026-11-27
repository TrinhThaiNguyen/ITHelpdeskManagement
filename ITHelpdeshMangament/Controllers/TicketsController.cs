
using ITHelpdeshMangament.Models.Tickets;
using ITHelpdeshMangament.Services.Tickets;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ITHelpdeshMangament.Controllers;

public class TicketsController : Controller
{
    private readonly TicketApiService _ticketApiService;

    public TicketsController(TicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    private bool IsAuthorized()
    {
        var token = HttpContext.Session.GetString("AccessToken");
        var expiresAt = HttpContext.Session.GetString("ExpiresAtUtc");
        var roles = HttpContext.Session.GetString("Roles");

        if (string.IsNullOrWhiteSpace(token) ||
            !DateTime.TryParse(
                expiresAt,
                null,
                DateTimeStyles.RoundtripKind,
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

        return isAuthorized;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        int? status,
        int? priority,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        var filters = new TicketListViewModel
        {
            SearchTerm = searchTerm,
            Status = status,
            Priority = priority,
            PageNumber = Math.Max(1, pageNumber),
            PageSize = Math.Clamp(pageSize, 5, 50)
        };

        try
        {
            var result = await _ticketApiService.GetTicketsAsync(
                filters,
                cancellationToken);

            return View(result);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the ticket service. Please try again later.";

            return View(filters);
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket service request timed out.";

            return View(filters);
        }
    }


    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            var ticket = await _ticketApiService.GetTicketByIdAsync(
                id,
                cancellationToken);

            if (ticket is null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to load ticket details. Please try again later.";

            return RedirectToAction(nameof(Index));
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket service request timed out.";

            return RedirectToAction(nameof(Index));
        }
    }

}
